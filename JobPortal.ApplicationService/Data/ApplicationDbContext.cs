using JobPortal.ApplicationService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.ApplicationService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<JobApplication> JobApplications { get; set; }
    }
}
