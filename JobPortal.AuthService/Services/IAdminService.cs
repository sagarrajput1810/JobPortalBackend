using JobPortal.AuthService.Models;

namespace JobPortal.AuthService.Services;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(Guid id);
    Task<bool> BlockUserAsync(Guid id);
    Task<bool> UnblockUserAsync(Guid id);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> ChangeUserRoleAsync(Guid id, string newRole);
    Task<AdminStatsDto> GetStatsAsync();
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalRecruiters { get; set; }
    public int BlockedUsers { get; set; }
}
