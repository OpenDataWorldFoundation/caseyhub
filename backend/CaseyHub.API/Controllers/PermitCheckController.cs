using System.Security.Claims;
using System.Text.Json;
using CaseyHub.API.Evaluators;
using CaseyHub.API.Repositories;
using CaseyHub.API.Services;
using CaseyHub.Models.DTOs.Internal.PermitChecker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseyHub.API.Controllers;

/// <summary>
/// Permit Checker — "Do I need a permit?" feature.
///
/// Endpoints:
///   GET  /api/permit-checker/building-types         — list of building types for the picker
///   POST /api/permit-checker/address                — step 1: address lookup + zone/overlay resolution
///   POST /api/permit-checker/evaluate               — step 2+: stateless rule evaluation
/// </summary>
[ApiController]
[Route("api/permit-checker")]
public class PermitCheckerController(
    IPermitCheckerAddressService addressService,
    IPermitEvaluatorService evaluatorService,
    IPermitCheckerRepository repo,
    ILogger<PermitCheckerController> logger) : ControllerBase
{
    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/permit-checker/building-types
    // Returns the list of building types for the frontend picker.
    // No auth required — this is public read-only reference data.
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet("building-types")]
    [ProducesResponseType(typeof(List<BuildingTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBuildingTypes()
    {
        var types = await repo.GetActiveBuildingTypesAsync();
        var dtos = types.Select(t => new BuildingTypeDto(
            t.Id, t.Slug, t.DisplayName, t.Description, t.DisplayOrder
        )).ToList();
        return Ok(dtos);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // POST /api/permit-checker/address
    // Step 1: User submits their address.
    // Nominatim geocodes it, VicPlan WFS resolves zone/overlays.
    // Returns session data and initial clauses for the sidebar.
    // ──────────────────────────────────────────────────────────────────────────
    [HttpPost("address")]
    [ProducesResponseType(typeof(AddressLookupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LookupAddress([FromBody] AddressLookupRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
            return BadRequest(new { message = "Address cannot be empty." });

        try
        {
            var result = await addressService.LookupAddressAsync(request.Address);

            if (result is null)
                return UnprocessableEntity(new
                {
                    message = "The address could not be geocoded. Please check the address and try again."
                });

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during permit checker address lookup");
            return StatusCode(500, new { message = "An unexpected error occurred during address lookup." });
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // POST /api/permit-checker/evaluate
    // Step 2+: Called after EVERY user answer (or after building type is selected).
    // The frontend sends the full accumulated context. Backend evaluates from scratch.
    // Returns either more questions or a conclusive verdict.
    //
    // Optional auth: if a JWT is present, the assessment is attributed to that user.
    // No JWT required — anonymous assessments are allowed.
    // ──────────────────────────────────────────────────────────────────────────
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(EvaluationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Evaluate([FromBody] EvaluationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BuildingTypeSlug))
            return BadRequest(new { message = "buildingTypeSlug is required." });

        if (string.IsNullOrWhiteSpace(request.ZoneCode))
            return BadRequest(new { message = "zoneCode is required. Ensure you have called /address first." });

        // Extract optional user ID from JWT if present
        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid parsedUserId))
            userId = parsedUserId;

        // Deserialise answers — the request carries Dictionary<string, object?> but
        // System.Text.Json deserialises object values as JsonElement. We normalise them
        // into their native types here so the evaluator can compare cleanly.
        var normalisedAnswers = NormaliseAnswers(request.Answers);

        var ctx = new EvaluationContext
        {
            SessionId         = request.SessionId,
            NormalisedAddress = request.NormalisedAddress,
            Latitude          = request.Latitude,
            Longitude         = request.Longitude,
            ZoneCode          = request.ZoneCode,
            OverlayCodes      = request.OverlayCodes ?? new List<string>(),
            BuildingTypeSlug  = request.BuildingTypeSlug,
            Answers           = normalisedAnswers
        };

        try
        {
            var result = await evaluatorService.EvaluateAsync(ctx, userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during permit evaluation");
            return StatusCode(500, new { message = "An unexpected error occurred during evaluation." });
        }
    }

    /// <summary>
    /// Normalises the answer values from System.Text.Json's JsonElement boxing
    /// into native .NET types (double, string, bool) so the ConditionEvaluator
    /// can compare them cleanly.
    /// </summary>
    private static Dictionary<string, object?> NormaliseAnswers(Dictionary<string, object?> raw)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in raw)
        {
            if (kvp.Value is JsonElement je)
            {
                result[kvp.Key] = je.ValueKind switch
                {
                    JsonValueKind.Number  => je.TryGetDouble(out double d) ? d : (object?)je.GetDecimal(),
                    JsonValueKind.String  => je.GetString(),
                    JsonValueKind.True    => true,
                    JsonValueKind.False   => false,
                    JsonValueKind.Null    => null,
                    _                    => je  // keep as JsonElement for array/object types
                };
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }
}