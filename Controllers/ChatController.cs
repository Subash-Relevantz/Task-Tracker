using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TrackerApi.Data;
using TrackerApi.Models;
using TrackerApi.Models.Chat;

namespace TrackerApi.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly TrackerDbContext _context;

    public ChatController(
        TrackerDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request)
    {
        var message =
            request.Message.ToLower().Trim();

        // ==================================================
        // USER AI - SAVE TASK
        // ==================================================

        if (
            message.StartsWith("today my task")
        )
        {
            var employee =
                await _context.AppUsers
                    .FirstOrDefaultAsync(
                        x => x.Name == "Subash"
                    );

            if (employee == null)
            {
                return Ok(new
                {
                    response =
                        "User not found."
                });
            }

            decimal hours = 1;

            var hourMatch =
                Regex.Match(
                    message,
                    @"(\d+(\.\d+)?)\s*hour"
                );

            if (hourMatch.Success)
            {
                hours =
                    Convert.ToDecimal(
                        hourMatch.Groups[1].Value
                    );
            }

            var entry =
                new TimeEntry
                {
                    UserId =
                        employee.Id,

                    WorkDate =
                        DateTime.Today,

                    FunctionalArea =
                        "Branding",

                    TaskCategory =
                        "Website",

                    TaskDescription =
                        request.Message,

                    Office =
                        "CAO",

                    ClusterOrg =
                        "Amazon",

                    Account =
                        "Org.BD.Marketing",

                    Hours =
                        hours
                };

            _context.TimeEntries.Add(
                entry);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                response =
                    $"✅ Task saved successfully ({hours}h)"
            });
        }

        // ==================================================
        // MANAGER AI
        // ==================================================

        var users =
            await _context.AppUsers
                .ToListAsync();

        var employeeUser =
            users.FirstOrDefault(u =>
                message.Contains(
                    u.Name.ToLower()
                ));

        // ==================================================
        // SHOW EMPLOYEE TASKS
        // ==================================================

        if (
            employeeUser != null &&
            (
                message.Contains("task") ||
                message.Contains("tasks")
            )
        )
        {
            var tasks =
                await _context.TimeEntries
                    .Where(x =>
                        x.UserId ==
                        employeeUser.Id
                    )
                    .OrderByDescending(x =>
                        x.WorkDate
                    )
                    .Take(10)
                    .ToListAsync();

            if (!tasks.Any())
            {
                return Ok(new
                {
                    response =
                        $"{employeeUser.Name} has no tasks."
                });
            }

            var result =
                string.Join(
                    "\n\n",
                    tasks.Select(x =>
                        $"📅 {x.WorkDate:dd-MM-yyyy}\n📝 {x.TaskDescription}\n⏱ {x.Hours}h"
                    )
                );

            return Ok(new
            {
                response =
                    result
            });
        }

        // ==================================================
        // SHOW EMPLOYEE HOURS
        // ==================================================

        if (
            employeeUser != null &&
            message.Contains("hours")
        )
        {
            var totalHours =
                await _context.TimeEntries
                    .Where(x =>
                        x.UserId ==
                        employeeUser.Id
                    )
                    .SumAsync(x =>
                        x.Hours
                    );

            return Ok(new
            {
                response =
                    $"{employeeUser.Name} has logged {totalHours} hours."
            });
        }

        // ==================================================
        // TEAM HOURS
        // ==================================================

        if (
            message.Contains("team")
        )
        {
            var teamHours =
                await _context.TimeEntries
                    .Include(x => x.User)
                    .GroupBy(x =>
                        x.User!.Name)
                    .Select(g => new
                    {
                        Name =
                            g.Key,

                        Hours =
                            g.Sum(x =>
                                x.Hours)
                    })
                    .OrderByDescending(x =>
                        x.Hours)
                    .ToListAsync();

            var response =
                string.Join(
                    "\n",
                    teamHours.Select(x =>
                        $"{x.Name}: {x.Hours}h")
                );

            return Ok(new
            {
                response
            });
        }

        // ==================================================
        // HELP
        // ==================================================

        return Ok(new
        {
            response =
@"USER COMMANDS

Task

MANAGER COMMANDS

Show Subash tasks
Show Stalin tasks
Show Kumaravel tasks

Show Subash hours
Show Stalin hours

Show team hours"
        });
    }
}