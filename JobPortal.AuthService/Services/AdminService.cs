using JobPortal.AuthService.Data;
using JobPortal.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.AuthService.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        return await _context.UserCredentials
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid id)
    {
        var u = await _context.UserCredentials.FindAsync(id);
        if (u == null) return null;
        return new AdminUserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt = u.CreatedAt
        };
    }

    // Block = set IsEmailVerified = false so user cannot login
    public async Task<bool> BlockUserAsync(Guid id)
    {
        var user = await _context.UserCredentials.FindAsync(id);
        if (user == null || user.Role == "Admin") return false;
        user.IsEmailVerified = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnblockUserAsync(Guid id)
    {
        var user = await _context.UserCredentials.FindAsync(id);
        if (user == null) return false;
        user.IsEmailVerified = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.UserCredentials.FindAsync(id);
        if (user == null || user.Role == "Admin") return false;
        _context.UserCredentials.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid id, string newRole)
    {
        var validRoles = new[] { "Candidate", "Recruiter" };
        if (!validRoles.Contains(newRole)) return false;

        var user = await _context.UserCredentials.FindAsync(id);
        if (user == null || user.Role == "Admin") return false;
        user.Role = newRole;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var all = await _context.UserCredentials.ToListAsync();
        return new AdminStatsDto
        {
            TotalUsers = all.Count,
            TotalCandidates = all.Count(u => u.Role == "Candidate"),
            TotalRecruiters = all.Count(u => u.Role == "Recruiter"),
            BlockedUsers = all.Count(u => !u.IsEmailVerified)
        };
    }
}
