using System.ComponentModel.DataAnnotations;

namespace JobPortal.ApplicationService.DTOs
{
    public class JobApplicationCreateDto
    {
        [Required]
        public int JobId { get; set; }
        public string ResumeUrl { get; set; } = string.Empty;
        public string? CoverLetter { get; set; }
    }

    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string CandidateId { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Applied";
        public DateTime AppliedDate { get; set; }
        public string? CoverLetter { get; set; }
        public int? AtsScore { get; set; }
        public string? AiSummary { get; set; }
    }

    public class JobApplicationStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
