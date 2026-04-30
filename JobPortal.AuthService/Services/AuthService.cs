using System.Text;
using JobPortal.AuthService.Data;
using JobPortal.AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using MassTransit;
using JobPortal.Shared.Events;
using Google.Apis.Auth;

namespace JobPortal.AuthService.Services;

public class AuthServices : IAuthServices
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthServices(ApplicationDbContext context, IConfiguration config, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _config = config;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<GoogleLoginResponse?> LoginWithGoogleAsync(string idToken, string? role = null)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _config["Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            
            var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email == payload.Email);
            
            if (user == null)
            {
                if (string.IsNullOrEmpty(role))
                {
                    // Return info to frontend so it can ask for role
                    return new GoogleLoginResponse 
                    { 
                        IsNewUser = true, 
                        Email = payload.Email, 
                        FullName = payload.Name 
                    };
                }

                // Create new user with selected role
                user = new UserCredential
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    PasswordHash = "GOOGLE_AUTH",
                    Role = role,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserCredentials.Add(user);
                await _context.SaveChangesAsync();
            }

            return new GoogleLoginResponse 
            { 
                Token = GenerateToken(user), 
                IsNewUser = false 
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request, string role)
    {
        var existingUser = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (existingUser != null)
        {
            if (existingUser.IsEmailVerified)
                return false; // Email truly exists and is verified

            // If user exists but NOT verified, update their info and send new OTP
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            existingUser.FullName = request.FullName;
            existingUser.Role = role;
            existingUser.VerificationOtp = new Random().Next(100000, 999999).ToString();
            existingUser.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
            
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new UserRegisteredEvent(existingUser.Email, existingUser.Role, existingUser.VerificationOtp));
            return true;
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        string otp = new Random().Next(100000, 999999).ToString();

        var newUser = new UserCredential
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = role,
            VerificationOtp = otp,
            OtpExpiry = DateTime.UtcNow.AddMinutes(15),
            IsEmailVerified = false
        };

        _context.UserCredentials.Add(newUser);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new UserRegisteredEvent(newUser.Email, newUser.Role, otp));

        return true;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        // Check if email is verified
        if (!user.IsEmailVerified)
        {
            throw new Exception("Email not verified. Please verify your email first.");
        }
        
        return GenerateToken(user);
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        if (user.VerificationOtp == otp && user.OtpExpiry > DateTime.UtcNow)
        {
            user.IsEmailVerified = true;
            user.VerificationOtp = null; // Clear OTP after verification
            user.OtpExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private string GenerateToken(UserCredential user)
    {
        var keyStr = _config["Jwt:Key"] ?? "default_secret_key_at_least_32_chars_long";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}