using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Business;

public class Recruiter : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Bio { get; set; }
    public Guid UserId { get; set; }

    public ICollection<Job> Jobs { get; set; } = [];
}
