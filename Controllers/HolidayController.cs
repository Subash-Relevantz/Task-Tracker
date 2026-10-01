using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerApi.Data;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HolidayController : ControllerBase
{
    private readonly TrackerDbContext _context;

    public HolidayController(
        TrackerDbContext context
    )
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult>
        GetHolidays()
    {
        var holidays =
            await _context.Holidays
                .OrderBy(x => x.HolidayDate)
                .ToListAsync();

        return Ok(holidays);
    }
}