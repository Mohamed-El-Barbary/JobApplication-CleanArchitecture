using JobApplication.Application.DTOs.Jobs;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.MappingConfigurations;

public class JobMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateJobCommand, Job>()
              .Map(dest => dest.Title, src => src.Title.Trim())
              .Map(dest => dest.Description, src => src.Description.Trim())
              .Map(dest => dest.Status, _ => JobStatus.Open)
              .Map(dest => dest.IsActive, _ => true)
              .Ignore(dest => dest.ClosedAt);

        config.NewConfig<Job, JobResponse>();
    }
}
