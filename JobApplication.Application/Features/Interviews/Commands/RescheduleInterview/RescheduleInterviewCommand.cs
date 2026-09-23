using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.RescheduleInterview;

public sealed record RescheduleInterviewCommand(
    DateTime ScheduledAt,
    int InterviewId,
    Guid UserId) : IRequest<Result>;
