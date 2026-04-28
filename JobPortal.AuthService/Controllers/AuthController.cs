using Microsoft.AspNetCore.Mvc;
using JobPortal.AuthService.Services;
using MassTransit;
using JobPortal.Shared.Events;

using System.Text.Json.Serialization;

namespace JobPortal.AuthService.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;
    private readonly IPublishEndpoint _publishEndpoint;
    public AuthController(IAuthServices authService, IPublishEndpoint publishEndpoint)
    {
        _authService = authService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("register/candidate")]
    public async Task<IActionResult> RegisterCandidate([FromBody] RegisterRequest request)
    {
        if (request == null) return BadRequest("Invalid request.");

        // Yahan hum explicitly "Candidate" bhej rahe hain
        var success = await _authService.RegisterAsync(request, "Candidate");
        if (!success) return BadRequest("Registration failed. Email might already exist.");

        Console.WriteLine($"Publishing UserRegisteredEvent for {request.Email}");
        await _publishEndpoint.Publish(new UserRegisteredEvent(request.Email, "Candidate"));

        return Ok("Candidate registered successfully!");
    }

    [HttpPost("register/recruiter")]
    public async Task<IActionResult> RegisterRecruiter([FromBody] RegisterRequest request)
    {
        if (request == null) return BadRequest("Invalid request.");

        // Yahan hum explicitly "Recruiter" bhej rahe hain
        var success = await _authService.RegisterAsync(request, "Recruiter");
        if (!success) return BadRequest("Registration failed. Email might already exist.");

        await _publishEndpoint.Publish(new UserRegisteredEvent(request.Email, "Recruiter"));

        return Ok("Recruiter registered successfully!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(new { Token = token });
    }
}

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;
}