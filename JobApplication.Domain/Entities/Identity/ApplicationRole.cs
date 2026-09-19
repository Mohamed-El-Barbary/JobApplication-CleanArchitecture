using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace JobApplication.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() => Id = Guid.NewGuid();
    public ApplicationRole(string name) : this() => Name = name;
    public string? Description { get; set; }
}
