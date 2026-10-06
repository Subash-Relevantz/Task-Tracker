namespace TrackerApi.Models.Chat;

public class SaveTaskRequest
{
    public int UserId { get; set; }

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
}