using System.Text.Json.Serialization;

namespace CaseyHub.Models.DTOs;

public record VicPlanningWfsResponse<T>
{
    [JsonPropertyName("features")]
    public List<VicPlanningFeature<T>> Features { get; init; } = [];

    [JsonPropertyName("totalFeatures")]
    public int? TotalFeatures { get; init; }

    [JsonPropertyName("numberMatched")]
    public int? NumberMatched { get; init; }

    [JsonPropertyName("numberReturned")]
    public int? NumberReturned { get; init; }

    [JsonPropertyName("timeStamp")]
    public string? TimeStamp { get; init; }
}

public record VicPlanningFeature<T>
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("geometry")]
    public object? Geometry { get; init; }

    [JsonPropertyName("properties")]
    public T Properties { get; init; } = default!;

    [JsonPropertyName("bbox")]
    public double[]? Bbox { get; init; }
}

public record PlanZoneProperties
{
    [JsonPropertyName("zone_code")]
    public string ZoneCode { get; init; } = string.Empty;

    [JsonPropertyName("zone_description")]
    public string ZoneDescription { get; init; } = string.Empty;
}

public record PlanOverlayProperties
{
    [JsonPropertyName("zone_code")]
    public string ZoneCode { get; init; } = string.Empty;
}

public record VicPlanZoneResult(
    string ZoneCode,
    string ZoneDescription,
    List<string> OverlayCodes
);
