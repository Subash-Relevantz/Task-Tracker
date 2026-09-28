namespace TrackerApi.DTOs;

public class ForgotPasswordRequest
{
    public string Email { get; set; }
        = string.Empty;

    public string NewPassword { get; set; }
        = string.Empty;
}