namespace JobApplication.Application.DTOs.Authentication;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmNewPassword { get; set; } = default!;
}
