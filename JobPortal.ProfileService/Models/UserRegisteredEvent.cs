namespace JobPortal.Shared.Events
{
    public record UserRegisteredEvent(Guid UserId, string Email, string FullName, string Role, string Otp);
}
