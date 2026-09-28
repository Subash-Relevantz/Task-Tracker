using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerApi.Data;
using TrackerApi.DTOs;
using TrackerApi.Services;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TrackerDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(
        TrackerDbContext db,
        TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var user =
            await _db.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Email.ToLower() == email &&
                    x.IsActive);

        if (user == null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid email or password."
            });
        }

        var valid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!valid)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid email or password."
            });
        }

        return Ok(new LoginResponse
        {
            Token =
                _tokens.CreateToken(user),

            UserId =
                user.Id,

            Name =
                user.Name,

            Email =
                user.Email,

            Role =
                user.Role
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var user =
            await _db.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Email.ToLower() == email &&
                    x.IsActive);

        if (user == null)
        {
            return BadRequest(new
            {
                message =
                    "User not found."
            });
        }

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                request.NewPassword);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Password reset successfully."
        });
    }
}