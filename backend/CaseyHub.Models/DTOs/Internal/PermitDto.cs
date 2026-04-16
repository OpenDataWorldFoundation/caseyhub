namespace CaseyHub.Models.DTOs.Internal;
public record PermitDto(
    string ApplicationNumber,
    string? ApplicationCategory,
    string? Description,
    string? Status,
    string? StageDecision,
    string? Address,
    DateTime? LodgedDate,
    DateTime? DecisionDate
);