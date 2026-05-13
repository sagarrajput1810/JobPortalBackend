using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.NotificationService.Data;
using JobPortal.NotificationService.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JobPortal.NotificationService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(NotificationDbContext context, ILogger<NotificationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

        try
        {
            var notifications = await _context.UserNotifications
                .Where(n => n.UserEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load notifications for {Email}", userEmail);
            return Ok(Array.Empty<UserNotification>());
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

        try
        {
            var count = await _context.UserNotifications
                .CountAsync(n => n.UserEmail == userEmail && !n.IsRead);

            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load unread notification count for {Email}", userEmail);
            return Ok(new { count = 0 });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        var notification = await _context.UserNotifications.FindAsync(id);

        if (notification == null || notification.UserEmail != userEmail)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

        var unreadNotifications = await _context.UserNotifications
            .Where(n => n.UserEmail == userEmail && !n.IsRead)
            .ToListAsync();

        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
