namespace CaseyHub.Models.DTOs.Internal.Auth;

public record RegisterRequestDto(
    string Name,
    string Email,
    string Password
);
