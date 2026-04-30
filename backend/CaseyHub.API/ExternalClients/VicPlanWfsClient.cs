using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CaseyHub.API.ExternalClients;

public class VicPlanWfsClient(HttpClient httpClient, IMemoryCache cache, ILogger<VicPlanWfsClient> logger ): IVicPlanWfsClient
{
    private const string WfsBaseUrl = "https://opendata.maps.vic.gov.au/geoserver/ows";
    private const string ZoneLayerName = "open-data-platform:plan_zone";
    private const string OverlayLayerName = "open-data-platform:plan_overlay";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<VicPlanZoneResult> GetZoneAndOverlaysAsync (double latitude, double longitude)
    {
        string cacheKey = $"vicplan:{latitude:F5}:{longitude:F5}";
        if (cache.TryGetValue(cacheKey, out VicPlanZoneResult? cached) && cached is not null)
        {
            logger.LogInformation("Cache HIT for {key}", cacheKey);
            return cached;
        }
        logger.LogInformation("Cache MISS - calling WFS for ({lat}, {lon})", latitude, longitude);

        var (zoneCode, zoneDescription) = await FetchZoneAsync(latitude, longitude);
        var overlayCodes = await FetchOverlaysAsync(latitude, longitude);

        var result = new VicPlanZoneResult(zoneCode, zoneDescription, overlayCodes);

        cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    private async Task<(string Code, string Description)> FetchZoneAsync(double latitude, double longitude)
    {
        try
        {
            string url = BuildWfsUrl(ZoneLayerName, latitude, longitude, "zone_code,zone_description");
            logger.LogDebug("VicPlan URL: {url} ", url);

            var response = await httpClient.GetFromJsonAsync<VicPlanningWfsResponse<PlanZoneProperties>>(url);
            var feature = response?.Features?.FirstOrDefault();
            if(feature?.Properties is null)
            {
                return ("UNKNOWN", "Zone Could Not Be Determined");
            }
            return (
                string.IsNullOrWhiteSpace(feature.Properties.ZoneCode) ? "UNKNOWN" : feature.Properties.ZoneCode,
                string.IsNullOrWhiteSpace(feature.Properties.ZoneDescription) ? "Unknown Zone" : feature.Properties.ZoneDescription
            );
        }catch(Exception ex)
        {
            logger.LogError(ex, " Failed to Fetch from VicPlan Zone ");
            return ("UNKNOWN", "Zone Lookup Failed");
        }
    }

    private async Task<List<string>> FetchOverlaysAsync (double latitude, double longitude)
    {
        try
        {
            string url = BuildWfsUrl(
                OverlayLayerName,
                latitude,
                longitude,
                "zone_code");

            logger.LogDebug("Overlay WFS URL: {Url}", url);

            var response = await httpClient.GetFromJsonAsync<VicPlanningWfsResponse<PlanOverlayProperties>>(url);

            var overlays = response?.Features?
                .Select(f => f.Properties.ZoneCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            return overlays ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch VicPlan overlays");
            return [];
        }
    }
    private static string BuildWfsUrl(string typeName, double latitude, double longitude, string properties)
    {
        string cql = FormattableString.Invariant($"INTERSECTS(geom,POINT({latitude} {longitude}))");
        return $"{WfsBaseUrl}" +
               $"?service=WFS" +
               $"&version=1.1.0" +
               $"&request=GetFeature" +
               $"&typeName={typeName}" +
               $"&srsName=EPSG:4326" +
               $"&cql_filter={Uri.EscapeDataString(cql)}" +
               $"&propertyName={properties}" +
               $"&outputFormat=application/json";
    }
}