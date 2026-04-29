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

    public async Task<bool> RegisterAsync(RegisterRequest request, string role)
    {
        if (await _context.UserCredentials.AnyAsync(u => u.Email == request.Email))
            return false;

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Generate a random 6-digit OTP
        string otp = new Random().Next(100000, 999999).ToString();

        var newUser = new UserCredential
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = role,
            VerificationOtp = otp,
            OtpExpiry = DateTime.UtcNow.AddMinutes(15), // OTP valid for 15 mins
            IsEmailVerified = false
        };

        _context.UserCredentials.Add(newUser);
        await _context.SaveChangesAsync();

        // Publish event to NotificationService to send email
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