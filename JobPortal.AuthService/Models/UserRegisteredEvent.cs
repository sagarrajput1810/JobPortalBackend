namespace JobPortal.Shared.Events
{
    public record UserRegisteredEvent(string Email, string Role, string Otp);
}