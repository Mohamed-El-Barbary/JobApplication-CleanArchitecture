using JobApplication.Application.DTOs.Authentication;
using Mapster;

namespace JobApplication.Application.MappingConfigurations;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuthUserDto, AuthenticationResponse>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Roles, src => src.Roles)
            .Map(dest => dest.AccessToken, src => src.AccessToken)
            .Map(dest => dest.AccessTokenExpiresAt, src => src.AccessTokenExpiresAt)
            .Map(dest => dest.RefreshToken, src => src.RefreshToken)
            .Map(dest => dest.RefreshTokenExpiresAt, src => src.RefreshTokenExpiresAt);
    }
}
