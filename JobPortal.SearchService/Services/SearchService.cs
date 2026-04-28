using JobPortal.SearchService.Models;

namespace JobPortal.SearchService.Services
{
    public class SearchService : ISearchService
    {
        // For demonstration purposes, we use an in-memory list. 
        // In a real application, you would use Elasticsearch, Meilisearch, or Lucene.
        private static readonly List<JobDocument> _jobs = new();

        public async Task<IEnumerable<JobDocument>> SearchJobsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _jobs;

            var lowerQuery = query.ToLower();
            var results = _jobs.Where(j =>
                j.Title.ToLower().Contains(lowerQuery) ||
                j.CompanyName.ToLower().Contains(lowerQuery) ||
                j.Location.ToLower().Contains(lowerQuery) ||
                j.Description.ToLower().Contains(lowerQuery)
            );

            return await Task.FromResult(results.ToList());
        }

        public async Task<bool> IndexJobAsync(JobDocument job)
        {
            var existing = _jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existing != null)
            {
                _jobs.Remove(existing);
            }
            _jobs.Add(job);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                _jobs.Remove(job);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> UpdateJobAsync(JobDocument job)
        {
            var existing = _jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existing != null)
            {
                _jobs.Remove(existing);
                _jobs.Add(job);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
    }
}
