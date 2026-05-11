using Microsoft.EntityFrameworkCore;
using JobPortal.SearchService.Models;

namespace JobPortal.SearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

    public DbSet<JobDocument> JobDocuments { get; set; }
}
