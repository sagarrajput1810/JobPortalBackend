using System.ComponentModel.DataAnnotations;

namespace JobPortal.JobService.Models
{
    public class JobPosting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        public string Location { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        [Required]
        public string RecruiterId { get; set; } = string.Empty; // Auth-Service se aane wala UserID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
