using System.Text.Json.Serialization;

namespace CaseyHub.Models.DTOs.External;

public record CaseyPermitResponse(
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("results")] CaseyPermitDto[] Results
);

public record CaseyPermitDto(
    [property: JsonPropertyName("application_number")] string ApplicationNumber,
    [property: JsonPropertyName("respauth")] string Decision,
    [property: JsonPropertyName("application_category")] string ApplicationCategory,
    [property: JsonPropertyName("stage_decision")] string DecisionStage,
    [property: JsonPropertyName("lodged_date")] string LodgedDate,
    [property: JsonPropertyName("decision_date")] string DecisionDate,
    [property: JsonPropertyName("advertise_commenced")] string AdvertiseCommncedDate, 
    [property: JsonPropertyName("advertise_completed")] string AdvertiseCompletedDate,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("suburb")] string Suburb,
    [property: JsonPropertyName("postcode")] string Postcode,
    [property: JsonPropertyName("ward")] string Ward,
    [property: JsonPropertyName("decision_new")] string DecisionNew,
    [property: JsonPropertyName("plnpermitaddress")] string PlanPermitAddress,
    [property: JsonPropertyName("serviceareadesc")] string ServiceAreaDescription,
    [property: JsonPropertyName("servicearea")] string ServiceAreaCode
);