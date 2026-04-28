using System.ComponentModel.DataAnnotations;
namespace JobPortal.ProfileService.Models;
public class RecruiterProfile
{
    [Key]
    public Guid Id {get; set;}
    [Required]
    public Guid UserId{get; set;}
    [Required]
    public string CompanyName {get; set;} = string.Empty;
    public string? ProfilePictureUrl {get; set;}
    public string? CompanyWebsite {get; set;}
    public string? Industry {get; set;} 
}