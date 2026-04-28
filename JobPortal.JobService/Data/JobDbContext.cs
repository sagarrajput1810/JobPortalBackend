using Microsoft.EntityFrameworkCore;
using JobPortal.JobService.Models;

namespace JobPortal.JobService.Data
{
    public class JobDbContext : DbContext
    {
        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options)
        {
        }

        public DbSet<JobPosting> JobPostings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Decimal Precision configuration for Salary
            modelBuilder.Entity<JobPosting>()
                .Property(j => j.Salary)
                .HasColumnType("decimal(18,2)");
        }
    }
}
