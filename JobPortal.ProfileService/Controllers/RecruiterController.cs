using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.ProfileService.Data;
using JobPortal.ProfileService.Models;

namespace JobPortal.ProfileService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecruiterController : ControllerBase
{
    private readonly ProfileDbContext _context;

    public RecruiterController(ProfileDbContext context)
    {
        _context = context;
    }

    // 1. CREATE: Naya Recruiter profile
    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] RecruiterProfile profile)
    {
        _context.RecruiterProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProfileById), new { id = profile.Id }, profile);
    }

    // 2. READ: Saare Recruiters
    [HttpGet]
    public async Task<IActionResult> GetAllProfiles()
    {
        var profiles = await _context.RecruiterProfiles.ToListAsync();
        return Ok(profiles);
    }

    // 3. READ: Specific Recruiter by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfileById(Guid id)
    {
        var profile = await _context.RecruiterProfiles.FindAsync(id);
        if (profile == null) return NotFound($"Recruiter with ID {id} not found.");
        return Ok(profile);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetProfileByUserId(Guid userId)
    {
        var profile = await _context.RecruiterProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    // 4. UPDATE: Company details update karna
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] RecruiterProfile updatedProfile)
    {
        var existingProfile = await _context.RecruiterProfiles.FindAsync(id);
        if (existingProfile == null) return NotFound();

        existingProfile.CompanyName = updatedProfile.CompanyName;
        existingProfile.Industry = updatedProfile.Industry;
        existingProfile.CompanyWebsite = updatedProfile.CompanyWebsite;
        existingProfile.ProfilePictureUrl = updatedProfile.ProfilePictureUrl; // Company Logo

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 5. DELETE: Remove profile
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var profile = await _context.RecruiterProfiles.FindAsync(id);
        if (profile == null) return NotFound();

        _context.RecruiterProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}