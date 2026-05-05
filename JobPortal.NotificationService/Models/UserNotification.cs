using System.ComponentModel.DataAnnotations;

namespace JobPortal.NotificationService.Models;

public class UserNotification
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserEmail { get; set; } = string.Empty; // Hum email ya UserId dono use kar sakte hain
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? RelatedUrl { get; set; } // Link to applications or jobs
}
