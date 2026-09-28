using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackerApi.Data;
using TrackerApi.DTOs;
using TrackerApi.Models;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/timeentries")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly TrackerDbContext _db;

    public TimeEntriesController(
        TrackerDbContext db)
    {
        _db = db;
    }


    // =====================================================
    // GET CURRENT LOGGED-IN USER ID FROM JWT
    // =====================================================

    private int CurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!int.TryParse(
            value,
            out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user information in token."
            );
        }

        return userId;
    }


    // =====================================================
    // GET CURRENT INDIA DATE
    // =====================================================

    private static DateTime IndiaToday()
    {
        try
        {
            var indiaTimeZone =
                TimeZoneInfo
                    .FindSystemTimeZoneById(
                        "Asia/Kolkata"
                    );

            return TimeZoneInfo
                .ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    indiaTimeZone
                )
                .Date;
        }
        catch
        {
            return DateTime.UtcNow
                .AddHours(5)
                .AddMinutes(30)
                .Date;
        }
    }


    // =====================================================
    // CREATE TIME ENTRY
    //
    // POST:
    // /api/timeentries
    //
    // Supports:
    // - Normal time entry
    // - Leave entry
    // - Selected historical date
    // - Maximum 24 hours per day
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] TimeEntryRequest request)
    {
        var userId =
            CurrentUserId();

        var today =
            IndiaToday();

        /*
         * IMPORTANT:
         * We use the date sent by React.
         *
         * Selecting 24/09/2026 therefore
         * saves 24/09/2026, not today's
         * date.
         */
        var requestedDate =
            request.WorkDate.Date;


        // ---------------------------------------------
        // VALIDATE DATE
        // ---------------------------------------------

        if (requestedDate > today)
        {
            return BadRequest(new
            {
                message =
                    "Future dates are not allowed."
            });
        }


        // ---------------------------------------------
        // LEAVE ENTRY
        // ---------------------------------------------

        if (request.IsLeave)
        {
            if (string.IsNullOrWhiteSpace(
                request.LeaveName))
            {
                return BadRequest(new
                {
                    message =
                        "Leave name is required."
                });
            }

            var leaveEntry =
                new TimeEntry
                {
                    UserId = userId,

                    WorkDate =
                        requestedDate,

                    FunctionalArea =
                        string.Empty,

                    TaskCategory =
                        string.Empty,

                    TaskDescription =
                        string.Empty,

                    Office =
                        string.Empty,

                    ClusterOrg =
                        string.Empty,

                    Account =
                        string.Empty,

                    Hours = 0,

                    IsLeave = true,

                    LeaveName =
                        request
                            .LeaveName
                            .Trim(),

                    CreatedAt =
                        DateTime.UtcNow,

                    UpdatedAt =
                        DateTime.UtcNow
                };

            _db.TimeEntries.Add(
                leaveEntry
            );

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Leave saved successfully.",

                id =
                    leaveEntry.Id,

                workDate =
                    leaveEntry
                        .WorkDate
                        .ToString(
                            "yyyy-MM-dd"
                        ),

                leaveEntry.IsLeave,

                leaveEntry.LeaveName,

                leaveEntry.Hours
            });
        }


        // ---------------------------------------------
        // NORMAL ENTRY REQUIRED FIELDS
        // ---------------------------------------------

        if (string.IsNullOrWhiteSpace(
            request.FunctionalArea))
        {
            return BadRequest(new
            {
                message =
                    "Functional Area is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.TaskCategory))
        {
            return BadRequest(new
            {
                message =
                    "Task Category is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.TaskDescription))
        {
            return BadRequest(new
            {
                message =
                    "Task Description is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.Office))
        {
            return BadRequest(new
            {
                message =
                    "Office is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.ClusterOrg))
        {
            return BadRequest(new
            {
                message =
                    "Cluster/Org is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.Account))
        {
            return BadRequest(new
            {
                message =
                    "Account is required."
            });
        }


        // ---------------------------------------------
        // VALIDATE HOURS
        // ---------------------------------------------

        if (
            request.Hours <= 0 ||
            request.Hours > 24)
        {
            return BadRequest(new
            {
                message =
                    "Hours must be greater than 0 and cannot exceed 24."
            });
        }


        // ---------------------------------------------
        // DAILY TOTAL MUST NOT EXCEED 24
        //
        // Leave entries are excluded.
        // ---------------------------------------------

        var existingHours =
            await _db.TimeEntries
                .Where(x =>
                    x.UserId == userId &&
                    x.WorkDate.Date ==
                        requestedDate &&
                    !x.IsLeave
                )
                .SumAsync(x =>
                    (decimal?)x.Hours
                ) ?? 0;

        if (
            existingHours +
            request.Hours > 24)
        {
            var availableHours =
                Math.Max(
                    0,
                    24 -
                    existingHours
                );

            return BadRequest(new
            {
                message =
                    $"You already logged {existingHours:0.##} hours for this date. Only {availableHours:0.##} hours are available."
            });
        }


        // ---------------------------------------------
        // CREATE NORMAL ENTRY
        // ---------------------------------------------

        var entry =
            new TimeEntry
            {
                UserId = userId,

                WorkDate =
                    requestedDate,

                FunctionalArea =
                    request
                        .FunctionalArea
                        .Trim(),

                TaskCategory =
                    request
                        .TaskCategory
                        .Trim(),

                TaskDescription =
                    request
                        .TaskDescription
                        .Trim(),

                Office =
                    request
                        .Office
                        .Trim(),

                ClusterOrg =
                    request
                        .ClusterOrg
                        .Trim(),

                Account =
                    request
                        .Account
                        .Trim(),

                Hours =
                    request.Hours,

                IsLeave = false,

                LeaveName = null,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };


        // ---------------------------------------------
        // SAVE
        // ---------------------------------------------

        _db.TimeEntries.Add(entry);

        await _db.SaveChangesAsync();


        // ---------------------------------------------
        // RESPONSE
        // ---------------------------------------------

        return Ok(new
        {
            message =
                "Entry saved successfully.",

            id =
                entry.Id,

            workDate =
                entry.WorkDate
                    .ToString(
                        "yyyy-MM-dd"
                    ),

            entry.FunctionalArea,

            entry.TaskCategory,

            entry.TaskDescription,

            entry.Office,

            entry.ClusterOrg,

            entry.Account,

            entry.Hours,

            entry.IsLeave,

            entry.LeaveName
        });
    }


    // =====================================================
    // UPDATE OWN ENTRY
    //
    // PUT:
    // /api/timeentries/{id}
    //
    // User can update ONLY their own entry.
    //
    // Supports:
    // - Task -> Task
    // - Task -> Leave
    // - Leave -> Task
    // - Leave -> Leave
    // =====================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] TimeEntryRequest request)
    {
        var userId =
            CurrentUserId();

        var today =
            IndiaToday();

        var requestedDate =
            request.WorkDate.Date;


        // ---------------------------------------------
        // FIND OWN ENTRY
        // ---------------------------------------------

        var entry =
            await _db.TimeEntries
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        x.UserId ==
                            userId
                );

        if (entry == null)
        {
            return NotFound(new
            {
                message =
                    "Entry not found."
            });
        }


        // ---------------------------------------------
        // VALIDATE DATE
        // ---------------------------------------------

        if (requestedDate > today)
        {
            return BadRequest(new
            {
                message =
                    "Future dates are not allowed."
            });
        }


        // ---------------------------------------------
        // UPDATE AS LEAVE
        // ---------------------------------------------

        if (request.IsLeave)
        {
            if (string.IsNullOrWhiteSpace(
                request.LeaveName))
            {
                return BadRequest(new
                {
                    message =
                        "Leave name is required."
                });
            }

            entry.WorkDate =
                requestedDate;

            entry.FunctionalArea =
                string.Empty;

            entry.TaskCategory =
                string.Empty;

            entry.TaskDescription =
                string.Empty;

            entry.Office =
                string.Empty;

            entry.ClusterOrg =
                string.Empty;

            entry.Account =
                string.Empty;

            entry.Hours = 0;

            entry.IsLeave = true;

            entry.LeaveName =
                request
                    .LeaveName
                    .Trim();

            entry.UpdatedAt =
                DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Leave updated successfully.",

                id =
                    entry.Id,

                workDate =
                    entry.WorkDate
                        .ToString(
                            "yyyy-MM-dd"
                        ),

                entry.IsLeave,

                entry.LeaveName,

                entry.Hours
            });
        }


        // ---------------------------------------------
        // NORMAL ENTRY REQUIRED FIELDS
        // ---------------------------------------------

        if (string.IsNullOrWhiteSpace(
            request.FunctionalArea))
        {
            return BadRequest(new
            {
                message =
                    "Functional Area is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.TaskCategory))
        {
            return BadRequest(new
            {
                message =
                    "Task Category is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.TaskDescription))
        {
            return BadRequest(new
            {
                message =
                    "Task Description is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.Office))
        {
            return BadRequest(new
            {
                message =
                    "Office is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.ClusterOrg))
        {
            return BadRequest(new
            {
                message =
                    "Cluster/Org is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.Account))
        {
            return BadRequest(new
            {
                message =
                    "Account is required."
            });
        }


        // ---------------------------------------------
        // VALIDATE HOURS
        // ---------------------------------------------

        if (
            request.Hours <= 0 ||
            request.Hours > 24)
        {
            return BadRequest(new
            {
                message =
                    "Hours must be greater than 0 and cannot exceed 24."
            });
        }


        // ---------------------------------------------
        // DAILY TOTAL VALIDATION
        //
        // Exclude:
        // - current entry being edited
        // - Leave entries
        // ---------------------------------------------

        var existingHours =
            await _db.TimeEntries
                .Where(x =>
                    x.UserId == userId &&
                    x.Id != id &&
                    x.WorkDate.Date ==
                        requestedDate &&
                    !x.IsLeave
                )
                .SumAsync(x =>
                    (decimal?)x.Hours
                ) ?? 0;

        if (
            existingHours +
            request.Hours > 24)
        {
            var availableHours =
                Math.Max(
                    0,
                    24 -
                    existingHours
                );

            return BadRequest(new
            {
                message =
                    $"You already logged {existingHours:0.##} other hours for this date. Only {availableHours:0.##} hours are available."
            });
        }


        // ---------------------------------------------
        // UPDATE NORMAL ENTRY
        // ---------------------------------------------

        entry.WorkDate =
            requestedDate;

        entry.FunctionalArea =
            request
                .FunctionalArea
                .Trim();

        entry.TaskCategory =
            request
                .TaskCategory
                .Trim();

        entry.TaskDescription =
            request
                .TaskDescription
                .Trim();

        entry.Office =
            request
                .Office
                .Trim();

        entry.ClusterOrg =
            request
                .ClusterOrg
                .Trim();

        entry.Account =
            request
                .Account
                .Trim();

        entry.Hours =
            request.Hours;

        entry.IsLeave = false;

        entry.LeaveName = null;

        entry.UpdatedAt =
            DateTime.UtcNow;


        // ---------------------------------------------
        // SAVE
        // ---------------------------------------------

        await _db.SaveChangesAsync();


        // ---------------------------------------------
        // RESPONSE
        // ---------------------------------------------

        return Ok(new
        {
            message =
                "Entry updated successfully.",

            id =
                entry.Id,

            workDate =
                entry.WorkDate
                    .ToString(
                        "yyyy-MM-dd"
                    ),

            entry.FunctionalArea,

            entry.TaskCategory,

            entry.TaskDescription,

            entry.Office,

            entry.ClusterOrg,

            entry.Account,

            entry.Hours,

            entry.IsLeave,

            entry.LeaveName
        });
    }


    // =====================================================
    // GET LOGGED-IN USER'S ENTRIES
    //
    // GET:
    // /api/timeentries/my
    // =====================================================

    [HttpGet("my")]
    public async Task<IActionResult>
        MyEntries()
    {
        var userId =
            CurrentUserId();

        var entries =
            await _db.TimeEntries
                .AsNoTracking()

                .Where(x =>
                    x.UserId ==
                    userId
                )

                .OrderByDescending(x =>
                    x.WorkDate
                )

                .ThenByDescending(x =>
                    x.CreatedAt
                )

                .Select(x => new
                {
                    x.Id,

                    x.UserId,

                    WorkDate =
                        x.WorkDate
                            .ToString(
                                "yyyy-MM-dd"
                            ),

                    x.FunctionalArea,

                    x.TaskCategory,

                    x.TaskDescription,

                    x.Office,

                    x.ClusterOrg,

                    x.Account,

                    x.Hours,

                    x.IsLeave,

                    x.LeaveName,

                    x.CreatedAt,

                    x.UpdatedAt
                })

                .ToListAsync();

        return Ok(entries);
    }


    // =====================================================
    // MANAGER DASHBOARD
    //
    // MANAGER ONLY
    //
    // GET:
    // /api/timeentries/dashboard
    // =====================================================

    [Authorize(Roles = "Manager")]
    [HttpGet("dashboard")]
    public async Task<IActionResult>
        ManagerDashboard()
    {
        var entries =
            await _db.TimeEntries
                .AsNoTracking()

                .Include(x =>
                    x.User
                )

                .OrderByDescending(x =>
                    x.WorkDate
                )

                .ThenBy(x =>
                    x.User.Name
                )

                .ThenByDescending(x =>
                    x.CreatedAt
                )

                .Select(x => new
                {
                    x.Id,

                    UserId =
                        x.UserId,

                    UserName =
                        x.User.Name,

                    UserEmail =
                        x.User.Email,

                    WorkDate =
                        x.WorkDate
                            .ToString(
                                "yyyy-MM-dd"
                            ),

                    x.FunctionalArea,

                    x.TaskCategory,

                    x.TaskDescription,

                    x.Office,

                    x.ClusterOrg,

                    x.Account,

                    x.Hours,

                    x.IsLeave,

                    x.LeaveName,

                    x.CreatedAt,

                    x.UpdatedAt
                })

                .ToListAsync();

        return Ok(entries);
    }


    // =====================================================
    // GET TODAY
    //
    // GET:
    // /api/timeentries/today
    // =====================================================

    [HttpGet("today")]
    public IActionResult Today()
    {
        var today =
            IndiaToday();

        return Ok(new
        {
            date =
                today.ToString(
                    "yyyy-MM-dd"
                )
        });
    }


    // =====================================================
    // DELETE OWN ENTRY
    //
    // DELETE:
    // /api/timeentries/{id}
    // =====================================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var userId =
            CurrentUserId();

        var entry =
            await _db.TimeEntries
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == id &&
                        x.UserId ==
                            userId
                );

        if (entry == null)
        {
            return NotFound(new
            {
                message =
                    "Entry not found."
            });
        }

        _db.TimeEntries.Remove(
            entry
        );

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Entry deleted successfully."
        });
    }
}