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
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger<AuthServices> _logger;

    public AuthServices(
        ApplicationDbContext context,
        IConfiguration config,
        ISendEndpointProvider sendEndpointProvider,
        ILogger<AuthServices> logger)
    {
        _context = context;
        _config = config;
        _sendEndpointProvider = sendEndpointProvider;
        _logger = logger;
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
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.");

        _logger.LogInformation("[AuthService] Attempting to register user: {Email}, Role: {Role}", request.Email, role);

        var existingUser = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            if (existingUser.IsEmailVerified)
            {
                _logger.LogWarning("[AuthService] Registration failed: Email {Email} already registered and verified.", request.Email);
                throw new Exception("Email is already registered. Please login.");
            }
            else
            {
                _logger.LogInformation("[AuthService] Email {Email} exists but not verified. Re-sending OTP.", request.Email);
                // If user exists but NOT verified, update their info and send new OTP
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                existingUser.FullName = request.FullName;
                existingUser.Role = role;
                existingUser.VerificationOtp = new Random().Next(100000, 999999).ToString();
                existingUser.OtpExpiry = DateTime.UtcNow.AddMinutes(15);

                await _context.SaveChangesAsync();

                try 
                {
                    await SendOtpEventAsync(existingUser.Id, existingUser.Email, existingUser.FullName, existingUser.Role, existingUser.VerificationOtp);
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "[AuthService] Failed to send OTP for existing unverified user {Email}", existingUser.Email);
                     throw new Exception($"Email already registered but not verified. Also failed to send new OTP: {ex.Message}");
                }

                throw new Exception("Email is already registered but not verified. A new OTP has been sent to your email.");
            }
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

        _logger.LogInformation("[AuthService] New user saved: {Email}. Sending OTP event...", newUser.Email);

        try
        {
            await SendOtpEventAsync(newUser.Id, newUser.Email, newUser.FullName, newUser.Role, otp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthService] Failed to send OTP event for new user {Email}", newUser.Email);
            // We don't rollback the user creation here to allow manual verification or re-registration attempt
            throw new Exception($"Registration saved, but OTP could not be sent: {ex.Message}. Please try verifying later.");
        }

        return true;
    }
    private async Task SendOtpEventAsync(Guid userId, string email, string fullName, string role, string otp)
    {
        var endpointUri = new Uri("queue:user-registered-event");
        try
        {
            _logger.LogInformation("Sending UserRegisteredEvent to {Endpoint} for {Email}", endpointUri, email);
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(endpointUri);
            await endpoint.Send(new UserRegisteredEvent(userId, email, fullName, role, otp));
            _logger.LogInformation("UserRegisteredEvent sent to {Endpoint} for {Email}", endpointUri, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send UserRegisteredEvent to {Endpoint} for {Email}", endpointUri, email);
            throw new Exception("Registration saved, but OTP could not be queued. Please try again later.");
        }
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
        var keyStr = _config["Jwt:Key"] 
            ?? throw new InvalidOperationException("Jwt:Key configuration is missing.");
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
