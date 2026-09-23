using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using JobApplication.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.UpdateInterview;

public sealed record UpdateInterviewCommand(
    DateTime ScheduledAt,
    TimeSpan Duration,
    InterviewType Type,
    string? MeetingUrl,
    string? Notes,
    int InterviewId,
    Guid UserId) : IRequest<Result<InterviewResponse>>;
