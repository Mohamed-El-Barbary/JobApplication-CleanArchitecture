using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure
}
