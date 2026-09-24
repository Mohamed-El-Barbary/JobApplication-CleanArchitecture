namespace JobApplication.Application.DTOs.Authentication;

using System;

public class VerifyEmailRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
}
