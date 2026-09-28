namespace TrackerApi.DTOs;

public class TimeEntryRequest
{
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
}