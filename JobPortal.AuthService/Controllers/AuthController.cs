using Microsoft.AspNetCore.Mvc;
using JobPortal.AuthService.Services;
using MassTransit;
using JobPortal.Shared.Events;

using System.Text.Json.Serialization;

using Microsoft.AspNetCore.RateLimiting;

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

        var success = await _authService.RegisterAsync(request, "Candidate");
        if (!success) return BadRequest("Registration failed. Email might already exist.");

        return Ok("Candidate registered successfully! Please check your email for OTP.");
    }

    [HttpPost("register/recruiter")]
    public async Task<IActionResult> RegisterRecruiter([FromBody] RegisterRequest request)
    {
        if (request == null) return BadRequest("Invalid request.");

        var success = await _authService.RegisterAsync(request, "Recruiter");
        if (!success) return BadRequest("Registration failed. Email might already exist.");

        return Ok("Recruiter registered successfully! Please check your email for OTP.");
    }

    [EnableRateLimiting("OtpPolicy")]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var success = await _authService.VerifyOtpAsync(request.Email, request.Otp);
        if (!success) return BadRequest("Invalid OTP or OTP expired.");

        return Ok("Email verified successfully! You can now login.");
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);
            if (token == null) return Unauthorized("Invalid email or password.");
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var response = await _authService.LoginWithGoogleAsync(request.IdToken, request.Role);
        if (response == null) return Unauthorized("Google authentication failed.");
        
        if (response.IsNewUser)
        {
            return Ok(new { IsNewUser = true, Email = response.Email, FullName = response.FullName });
        }

        return Ok(new { Token = response.Token });
    }
}

public class GoogleLoginRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class VerifyEmailRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;
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