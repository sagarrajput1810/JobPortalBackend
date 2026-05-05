using Microsoft.EntityFrameworkCore;
using JobPortal.NotificationService.Models;

namespace JobPortal.NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<UserNotification> UserNotifications { get; set; }
}
