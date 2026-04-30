using CaseyHub.API.ExternalClients;
using CaseyHub.API.Repositories;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.Internal.PermitChecker;
using Microsoft.Extensions.Logging;

namespace CaseyHub.API.Services;

public interface IPermitCheckerAddressService
{
    Task<AddressLookupResponseDto?> LookupAddressAsync(string rawAddress);
}

/// <summary>
/// Handles the first step of the permit checker:
///   1. Normalise the address using Nominatim (reuses the existing INominatimClient)
///   2. Call VicPlan WFS to get zone + overlays for the resolved coordinates
///   3. Fetch the initially-relevant planning clauses for the zone
///   4. Return the full AddressLookupResponseDto
/// </summary>
public class PermitCheckerAddressService(
    INominatimClient nominatimClient,
    IVicPlanWfsClient vicPlanClient,
    IPermitCheckerRepository repo,
    ILogger<PermitCheckerAddressService> logger) : IPermitCheckerAddressService
{
    public async Task<AddressLookupResponseDto?> LookupAddressAsync(string rawAddress)
    {
        // ── Step 1: Geocode the address using Nominatim ──────────────────────
        logger.LogInformation("PermitChecker address lookup: '{Address}'", rawAddress);

        var geoResult = await nominatimClient.EnrichAddressAsync(rawAddress, usePrivateServer: false);

        if (geoResult is null
            || !double.TryParse(geoResult.Latitude, out double lat)
            || !double.TryParse(geoResult.Longitude, out double lon))
        {
            logger.LogWarning("Nominatim returned no result for address: {Address}", rawAddress);
            return null;
        }

        // Build the normalised address string from the structured response
        string normalisedAddress = BuildNormalisedAddress(geoResult.Address);
        logger.LogInformation("Geocoded to ({Lat},{Lon}): '{Normalised}'", lat, lon, normalisedAddress);

        // ── Step 2: Call VicPlan WFS ──────────────────────────────────────────
        var vicPlanResult = await vicPlanClient.GetZoneAndOverlaysAsync(lat, lon);
        logger.LogInformation(
            "VicPlan result: Zone={Zone}, Overlays={Overlays}",
            vicPlanResult.ZoneCode, string.Join(",", vicPlanResult.OverlayCodes));

        // ── Step 3: Get initially-relevant clauses for this zone ──────────────
        var clauses = await repo.GetClausesForZoneAsync(vicPlanResult.ZoneCode);

        // ── Step 4: Build and return response ────────────────────────────────
        return new AddressLookupResponseDto(
            SessionId: Guid.NewGuid().ToString(),
            NormalisedAddress: normalisedAddress,
            Latitude: lat,
            Longitude: lon,
            ZoneCode: vicPlanResult.ZoneCode,
            ZoneDescription: vicPlanResult.ZoneDescription,
            OverlayCodes: vicPlanResult.OverlayCodes,
            RelevantClauses: clauses
                .Select(c => new ClauseDto(c.ClauseNumber, c.Title, c.Summary, c.OfficialUrl))
                .ToList()
        );
    }

    private static string BuildNormalisedAddress(CaseyHub.Models.DTOs.External.NominatimLocationDto addr)
    {
        var parts = new[]
        {
            addr.HouseNumber,
            addr.Street,
            addr.Suburb,
            addr.State,
            addr.Postcode
        }.Where(p => !string.IsNullOrWhiteSpace(p));

        return string.Join(", ", parts);
    }
}