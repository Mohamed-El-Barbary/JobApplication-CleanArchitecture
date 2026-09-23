using JobApplication.Application.DTOs.Interviews;
using JobApplication.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Interviews.Queries.GetMyInterviews;

public sealed record GetMyInterviewsQuery(Guid UserId) : IRequest<Result<IEnumerable<InterviewResponse>>>;
