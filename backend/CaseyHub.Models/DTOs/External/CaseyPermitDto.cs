using System.Text.Json.Serialization;

namespace CaseyHub.Models.DTOs.External;

public record CaseyPermitResponse(
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("results")] CaseyPermitDto[] Results
);

public record CaseyPermitDto(
    [property: JsonPropertyName("application_number")] string ApplicationNumber,
    [property: JsonPropertyName("application_category")] string? ApplicationCategory,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("respauth")] string? Status,
    [property: JsonPropertyName("stage_decision")] string? StageDecision,
    [property: JsonPropertyName("plnpermitaddress")] string? PlanPermitAddress, 
    [property: JsonPropertyName("lodged_date")] string? LodgedDate, 
    [property: JsonPropertyName("decision_date")] string? DecisionDate
);