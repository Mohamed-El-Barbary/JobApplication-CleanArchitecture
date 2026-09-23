using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.ScheduleInterview;

public sealed record ScheduleInterviewCommand(
    DateTime ScheduledAt,
    TimeSpan Duration,
    InterviewType Type,
    string? MeetingUrl,
    string? Notes,
    int ApplicationId,
    Guid UserId) : IRequest<Result<InterviewResponse>>;
