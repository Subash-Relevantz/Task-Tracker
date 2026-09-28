using Microsoft.EntityFrameworkCore;
using TrackerApi.Models;

namespace TrackerApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        TrackerDbContext db)
    {
        if (await db.AppUsers.AnyAsync())
        {
            return;
        }

        const string temporaryPassword =
            "Tracker@123";

        var users = new[]
        {
            new AppUser
            {
                Name = "Kumaravel",
                Email = "kumaravel@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Enbin",
                Email = "enbin@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Deepak",
                Email = "deepak@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Stalin",
                Email = "stalin@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Amirtha",
                Email = "amirtha@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Emman",
                Email = "emman@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Arunachalam",
                Email = "arunachalam@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Subash",
                Email = "subash@tracker.local",
                Role = "User"
            },

            new AppUser
            {
                Name = "Varun",
                Email = "varun@tracker.local",
                Role = "Manager"
            }
        };

        foreach (var user in users)
        {
            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    temporaryPassword
                );
        }

        await db.AppUsers.AddRangeAsync(users);

        await db.SaveChangesAsync();
    }
}