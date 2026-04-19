namespace CaseyHub.Models.DTOs.Internal.Auth;

public record AuthResultDto(
    bool Succeeded,
    string? ErrorMessage,
    AuthResponseDto? Response
);
