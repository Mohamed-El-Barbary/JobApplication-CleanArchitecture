using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Business;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
}
