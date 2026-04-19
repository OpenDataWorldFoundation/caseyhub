using CaseyHub.Models.DTOs.Internal.Auth;

namespace CaseyHub.API.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
}
