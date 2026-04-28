using System.Text;
using JobPortal.AuthService.Data;
using JobPortal.AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace JobPortal.AuthService.Services;

public class AuthServices : IAuthServices
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    public AuthServices(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    public async Task<bool> RegisterAsync(RegisterRequest request, string role)
    {
        // 1. Check if user exists (Db se check karo)
        if (await _context.UserCredentials.AnyAsync(u => u.Email == request.Email))
            return false;

        // 2. Hash the password (DTO se password uthaya)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Map DTO to Entity (Yahan controller ka kaam khatam)
        var newUser = new UserCredential
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = role
        };

        _context.UserCredentials.Add(newUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        
        return GenerateToken(user);
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