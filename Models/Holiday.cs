namespace TrackerApi.Models;

public class Holiday
{
    public int Id { get; set; }

    public DateTime HolidayDate { get; set; }

    public string HolidayName { get; set; } = "";
}