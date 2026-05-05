using System.ComponentModel.DataAnnotations;

namespace JobPortal.ApplicationService.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; } // JobService wala Id

        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string CandidateId { get; set; } = string.Empty; // AuthService wala UserID

        [Required]
        public string CandidateName { get; set; } = string.Empty;

        [Required]
        public string CandidateEmail { get; set; } = string.Empty;

        public string ResumeUrl { get; set; } = string.Empty;

        public string Status { get; set; } = "Applied"; // Applied, Under Review, Interviewed, Selected, Rejected

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        public string? CoverLetter { get; set; }

        public int? AtsScore { get; set; } // Added for AI Service

        public string? AiSummary { get; set; } // Added for AI Service
    }
}
