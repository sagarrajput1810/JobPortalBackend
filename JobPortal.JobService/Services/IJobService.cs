using JobPortal.JobService.DTOs;

namespace JobPortal.JobService.Services
{
    public interface IJobService
    {
        Task<IEnumerable<JobResponseDto>> GetAllJobsAsync();
        Task<JobResponseDto?> GetJobByIdAsync(int id);
        Task<JobResponseDto> CreateJobAsync(JobCreateDto jobCreateDto, string recruiterId);
        Task<bool> UpdateJobAsync(int id, JobUpdateDto jobUpdateDto);
        Task<bool> DeleteJobAsync(int id);
        Task<IEnumerable<JobResponseDto>> GetJobsByRecruiterAsync(string recruiterId);
    }
}
