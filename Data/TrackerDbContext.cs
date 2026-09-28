using Microsoft.EntityFrameworkCore;
using TrackerApi.Models;

namespace TrackerApi.Data;

public class TrackerDbContext : DbContext
{
    public TrackerDbContext(
        DbContextOptions<TrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<TimeEntry>()
            .HasOne(x => x.User)
            .WithMany(x => x.TimeEntries)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeEntry>()
            .Property(x => x.Hours)
            .HasPrecision(5, 2);

        modelBuilder.Entity<TimeEntry>()
            .HasIndex(x => new
            {
                x.UserId,
                x.WorkDate
            });
    }
}