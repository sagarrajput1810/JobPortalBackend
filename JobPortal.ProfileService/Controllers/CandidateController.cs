using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.ProfileService.Data;
using JobPortal.ProfileService.Models;

namespace JobPortal.ProfileService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidateController : ControllerBase
{
    private readonly ProfileDbContext _context;

    public CandidateController(ProfileDbContext context)
    {
        _context = context;
    }

    // 1. CREATE: Naya profile banana
    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CandidateProfile profile)
    {
        _context.CandidateProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProfileById), new { id = profile.Id }, profile);
    }

    // 2. READ: Saare profiles fetch karna
    [HttpGet]
    public async Task<IActionResult> GetAllProfiles()
    {
        var profiles = await _context.CandidateProfiles.ToListAsync();
        return Ok(profiles);
    }

    // 3. READ: Specific profile fetch karna (by ID)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfileById(Guid id)
    {
        var profile = await _context.CandidateProfiles.FindAsync(id);
        if (profile == null) return NotFound($"Candidate with ID {id} not found.");
        return Ok(profile);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetProfileByUserId(Guid userId)
    {
        var profile = await _context.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    // 4. UPDATE: Profile update karna
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] CandidateProfile updatedProfile)
    {
        var existingProfile = await _context.CandidateProfiles.FindAsync(id);
        if (existingProfile == null) return NotFound();

        existingProfile.FullName = updatedProfile.FullName;
        existingProfile.Bio = updatedProfile.Bio;
        existingProfile.PhoneNumber = updatedProfile.PhoneNumber;
        existingProfile.Skills = updatedProfile.Skills;
        // ResumeUrl aur PictureUrl bhi update kar sakte ho...

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 5. DELETE: Profile delete karna
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var profile = await _context.CandidateProfiles.FindAsync(id);
        if (profile == null) return NotFound();

        _context.CandidateProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}