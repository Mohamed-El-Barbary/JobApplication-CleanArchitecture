using JobApplication.Application.DTOs.Candidates;
using JobApplication.Domain.Entities.Business;
using Mapster;

namespace JobApplication.Application.MappingConfigurations;

public class CandidateMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Candidate, CandidateProfileResponse>();

        config.NewConfig<UpdateCandidateProfileRequest, Candidate>()
            .IgnoreNullValues(true);
    }
}
