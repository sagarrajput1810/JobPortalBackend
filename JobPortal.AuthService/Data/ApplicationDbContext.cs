using Microsoft.EntityFrameworkCore;
using JobPortal.AuthService.Models;

namespace JobPortal.AuthService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<UserCredential> UserCredentials { get; set;}
}