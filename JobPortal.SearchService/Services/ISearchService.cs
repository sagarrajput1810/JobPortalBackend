using JobPortal.SearchService.Models;

namespace JobPortal.SearchService.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<JobDocument>> SearchJobsAsync(string query);
        Task<bool> IndexJobAsync(JobDocument job);
        Task<bool> DeleteJobAsync(int id);
        Task<bool> UpdateJobAsync(JobDocument job);
    }
}
