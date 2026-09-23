using JobApplication.Application.DTOs.JobApplications;
using JobApplication.Application.Features.JobApplications.Commands.CreateJobApplication;
using JobApplication.Application.Features.JobApplications.Commands.UpdateJobApplication;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Enums;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.MappingConfigurations
{
    internal class JobApplicationMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateJobApplicationCommand, JobCandidateApplication>()
                .Map(dest => dest.CoverLetter,
                    src => src.CoverLetter == null
                        ? null
                        : src.CoverLetter.Trim())
                .Map(dest => dest.ResumeUrl,
                    src => src.ResumeUrl == null
                        ? null
                        : src.ResumeUrl.Trim())
                .Map(dest => dest.Status, _ => ApplicationStatus.Applied);

            config.NewConfig<JobCandidateApplication, JobApplicationResponse>();

            config.NewConfig<UpdateJobApplicationCommand, JobCandidateApplication>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.CandidateId)
                .Ignore(dest => dest.JobId)
                .Ignore(dest => dest.Status)
                .Ignore(dest => dest.CreatedAt)
                .Map(dest => dest.CoverLetter, 
                    src => src.CoverLetter == null 
                        ? null 
                        : src.CoverLetter.Trim())
                .Map(dest => dest.ResumeUrl, 
                    src => src.ResumeUrl == null 
                        ? null 
                        : src.ResumeUrl.Trim());
        }
    }
}
