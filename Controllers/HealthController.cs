using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerApi.Data;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly TrackerDbContext _dbContext;

    public HealthController(
        TrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "TrackerApi"
        });
    }


    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            var canConnect =
                await _dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        status = "Failed",
                        database = "TrackerDb",
                        message = "Unable to connect to MySQL."
                    }
                );
            }

            return Ok(new
            {
                status = "Connected",
                database = "TrackerDb",
                provider =
                    _dbContext.Database.ProviderName
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    status = "Failed",
                    database = "TrackerDb",
                    message = ex.Message
                }
            );
        }
    }
}