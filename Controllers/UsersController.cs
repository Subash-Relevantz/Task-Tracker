using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerApi.Data;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly TrackerDbContext _db;

    public UsersController(
        TrackerDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("team")]
    public async Task<IActionResult> Team()
    {
        var members =
            await _db.AppUsers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Role
                })
                .ToListAsync();

        return Ok(members);
    }
}