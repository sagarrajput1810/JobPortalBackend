using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobPortal.AuthService.Services;
using System.Text.Json.Serialization;

namespace JobPortal.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // GET /api/admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET /api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null) return NotFound("User not found.");
        return Ok(user);
    }

    // PUT /api/admin/users/{id}/block
    [HttpPut("users/{id}/block")]
    public async Task<IActionResult> BlockUser(Guid id)
    {
        var result = await _adminService.BlockUserAsync(id);
        if (!result) return BadRequest("Cannot block this user.");
        return Ok("User blocked successfully.");
    }

    // PUT /api/admin/users/{id}/unblock
    [HttpPut("users/{id}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id)
    {
        var result = await _adminService.UnblockUserAsync(id);
        if (!result) return NotFound("User not found.");
        return Ok("User unblocked successfully.");
    }

    // DELETE /api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _adminService.DeleteUserAsync(id);
        if (!result) return BadRequest("Cannot delete this user.");
        return Ok("User deleted successfully.");
    }

    // PUT /api/admin/users/{id}/role
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var result = await _adminService.ChangeUserRoleAsync(id, request.NewRole);
        if (!result) return BadRequest("Invalid role or cannot change Admin's role.");
        return Ok($"User role changed to {request.NewRole}.");
    }
}

public class ChangeRoleRequest
{
    [JsonPropertyName("newRole")]
    public string NewRole { get; set; } = string.Empty;
}
