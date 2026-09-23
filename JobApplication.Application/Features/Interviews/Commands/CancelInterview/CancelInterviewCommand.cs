using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Commands.CancelInterview;

public sealed record CancelInterviewCommand(
    int InterviewId,
    Guid UserId) : IRequest<Result>;
