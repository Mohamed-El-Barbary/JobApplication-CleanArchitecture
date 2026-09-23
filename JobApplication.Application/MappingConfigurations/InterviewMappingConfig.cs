using JobApplication.Application.DTOs.Interviews;
using JobApplication.Application.Features.Interviews.Commands.ScheduleInterview;
using JobApplication.Application.Features.Interviews.Commands.UpdateInterview;
using JobApplication.Domain.Entities.Business;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.MappingConfigurations;

internal class InterviewMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ScheduleInterviewCommand, Interview>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.JobApplicationId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Map(dest => dest.MeetingUrl,
                src => src.MeetingUrl == null ? null : src.MeetingUrl.Trim())
            .Map(dest => dest.Notes,
                src => src.Notes == null ? null : src.Notes.Trim());

        config.NewConfig<UpdateInterviewCommand, Interview>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.JobApplicationId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Map(dest => dest.MeetingUrl,
                src => src.MeetingUrl == null ? null : src.MeetingUrl.Trim())
            .Map(dest => dest.Notes,
                src => src.Notes == null ? null : src.Notes.Trim());

        config.NewConfig<Interview, InterviewResponse>();
    }
}
