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
        Name = "Stalin",
        Email = "stalin.vaithilingam@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Varun",
        Email = "dayananth.varun@relevantz.com",
        Role = "Manager"
    },

    new AppUser
    {
        Name = "Sathish",
        Email = "sathish.hariharan@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Emmanuel",
        Email = "emmanuel.davidson@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Arunachalam",
        Email = "arunachalam.sivakumar@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Deepak",
        Email = "deepak.thokhuluva@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Enbin",
        Email = "enbin.susainathan@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Amirthavarshini",
        Email = "amirthavarshini.prasanna@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Kumaravel",
        Email = "kumaravel.thiyagarajan@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Subash",
        Email = "subash.rajasekaran@relevantz.com",
        Role = "User"
    },

    new AppUser
    {
        Name = "Kishore",
        Email = "kishore.kanthasamy@relevantz.com",
        Role = "User"
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