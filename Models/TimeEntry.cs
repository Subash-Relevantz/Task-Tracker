namespace TrackerApi.Models;

public class TimeEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime WorkDate { get; set; }

    public string FunctionalArea { get; set; }
        = string.Empty;

    public string TaskCategory { get; set; }
        = string.Empty;

    public string TaskDescription { get; set; }
        = string.Empty;

    public string Office { get; set; }
        = string.Empty;

    public string ClusterOrg { get; set; }
        = string.Empty;

    public string Account { get; set; }
        = string.Empty;

    public decimal Hours { get; set; }

    public bool IsLeave { get; set; }

    public string? LeaveName { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public AppUser? User { get; set; }
}