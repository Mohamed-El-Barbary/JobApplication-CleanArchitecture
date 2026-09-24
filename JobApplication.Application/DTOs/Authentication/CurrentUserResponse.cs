namespace JobApplication.Application.DTOs.Authentication;

using System;
using System.Collections.Generic;

public class CurrentUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
