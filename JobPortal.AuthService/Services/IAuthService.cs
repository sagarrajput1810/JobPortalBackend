using JobPortal.AuthService.Models;

namespace JobPortal.AuthService.Services;
public interface IAuthServices
{
    Task<bool> RegisterAsync(RegisterRequest request, string role);
    Task<string?> LoginAsync(string email, string password);
    Task<bool> VerifyOtpAsync(string email, string otp);
}