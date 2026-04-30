using JobPortal.AuthService.Models;

namespace JobPortal.AuthService.Services;
public interface IAuthServices
{
    Task<bool> RegisterAsync(RegisterRequest request, string role);
    Task<string?> LoginAsync(string email, string password);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task<GoogleLoginResponse?> LoginWithGoogleAsync(string idToken, string? role = null);
}

public class GoogleLoginResponse
{
    public string? Token { get; set; }
    public bool IsNewUser { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
}