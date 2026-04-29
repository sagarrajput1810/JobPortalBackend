namespace JobPortal.AuthService.Models;

public class UserCredential
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string FullName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public string Role{get; set;} = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public string? VerificationOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}