using System.Text.Json.Serialization;

namespace CaseyHub.Models.DTOs.External;

public record GeoCodeResponse
(
    [property: JsonPropertyName("lat")] string Latitude,
    [property: JsonPropertyName("lon")] string Longitude,
    [property: JsonPropertyName("address")] NominatimLocationDto Address
);

public record NominatimLocationDto
(
    [property: JsonPropertyName("house_number")] string HouseNumber,
    [property: JsonPropertyName("road")] string Street,
    [property: JsonPropertyName("suburb")] string Suburb,
    [property: JsonPropertyName("municipality")] string Municipality,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("postcode")] string Postcode,
    [property: JsonPropertyName("country")] string Country
);