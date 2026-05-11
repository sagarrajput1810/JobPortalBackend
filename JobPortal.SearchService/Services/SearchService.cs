using Microsoft.EntityFrameworkCore;
using JobPortal.SearchService.Data;
using JobPortal.SearchService.Models;

namespace JobPortal.SearchService.Services
{
    public class SearchService : ISearchService
    {
        private readonly SearchDbContext _context;

        public SearchService(SearchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobDocument>> SearchJobsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await _context.JobDocuments.ToListAsync();

            var lowerQuery = query.ToLower();
            return await _context.JobDocuments
                .Where(j =>
                    j.Title.ToLower().Contains(lowerQuery) ||
                    j.CompanyName.ToLower().Contains(lowerQuery) ||
                    j.Location.ToLower().Contains(lowerQuery) ||
                    j.Description.ToLower().Contains(lowerQuery)
                ).ToListAsync();
        }

        public async Task<bool> IndexJobAsync(JobDocument job)
        {
            var existing = await _context.JobDocuments.FindAsync(job.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(job);
            }
            else
            {
                _context.JobDocuments.Add(job);
            }
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _context.JobDocuments.FindAsync(id);
            if (job != null)
            {
                _context.JobDocuments.Remove(job);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> UpdateJobAsync(JobDocument job)
        {
            var existing = await _context.JobDocuments.FindAsync(job.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(job);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
