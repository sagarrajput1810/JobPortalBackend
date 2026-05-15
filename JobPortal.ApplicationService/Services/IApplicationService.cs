using JobPortal.ApplicationService.DTOs;

namespace JobPortal.ApplicationService.Services
{
    public interface IApplicationService
    {
        Task<JobApplicationResponseDto> ApplyForJobAsync(JobApplicationCreateDto applicationDto, string candidateId, string candidateName, string candidateEmail);
        Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByJobIdAsync(int jobId);
        Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCandidateIdAsync(string candidateId);
        Task<bool> UpdateApplicationStatusAsync(int id, string status);
        Task<JobApplicationResponseDto?> GetApplicationByIdAsync(int id);
        Task<IEnumerable<JobApplicationResponseDto>> GetAllApplicationsAsync();
        Task<bool> DeleteApplicationAsync(int id);
    }
}
