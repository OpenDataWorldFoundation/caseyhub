namespace CaseyHub.Models.DTOs.Internal.Auth;

public record LoginRequestDto(
    string Email,
    string Password
);
