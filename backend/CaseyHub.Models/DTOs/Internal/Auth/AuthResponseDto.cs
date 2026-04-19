namespace CaseyHub.Models.DTOs.Internal.Auth;

public record AuthResponseDto(
    Guid UserId,
    string Name,
    string Email,
    string Token,
    DateTime ExpiresAtUtc
);
