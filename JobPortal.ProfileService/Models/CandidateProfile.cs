using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JobPortal.ProfileService.Models;

public class CandidateProfile
{
    [Key]
    [JsonRequired]
    public Guid Id {get; set;}

    [Required]
    [JsonRequired]
    public Guid UserId {get; set;}

    [Required(ErrorMessage ="Full Name is required")]
    [StringLength(100, MinimumLength =3)]
    public string FullName {get; set;} = string.Empty;
    [Required(ErrorMessage ="Phone Number is required")]
    [Phone(ErrorMessage ="Invalid Phone Number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage ="Phone number must be exactly 10 digits")]
    public string PhoneNumber {get; set;} = string.Empty;
    public string? Bio {get; set;}
    public string? ProfilePictureUrl {get; set;}
    public string? ResumeUrl {get; set;}
    public List<string> Skills {get; set;} = new();
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}
