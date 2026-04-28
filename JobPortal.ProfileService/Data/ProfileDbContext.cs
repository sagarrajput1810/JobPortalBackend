using Microsoft.EntityFrameworkCore;
using JobPortal.ProfileService.Models;

namespace JobPortal.ProfileService.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }

    // Yeh DbSet hi aapke SQL Table banege
    public DbSet<CandidateProfile> CandidateProfiles { get; set; }
    public DbSet<RecruiterProfile> RecruiterProfiles { get; set; }
}