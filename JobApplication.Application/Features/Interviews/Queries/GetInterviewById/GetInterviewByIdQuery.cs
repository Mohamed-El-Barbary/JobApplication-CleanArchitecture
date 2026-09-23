using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Queries.GetInterviewById;

public sealed record GetInterviewByIdQuery(
    int InterviewId,
    Guid UserId) : IRequest<Result<InterviewResponse>>;
