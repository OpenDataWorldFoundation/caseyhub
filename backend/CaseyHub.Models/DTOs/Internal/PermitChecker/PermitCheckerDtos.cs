using System.Text.Json.Serialization;

namespace CaseyHub.Models.DTOs.Internal.PermitChecker;

// ─────────────────────────────────────────────
// REQUEST DTOs (frontend → backend)
// ─────────────────────────────────────────────

/// <summary>
/// Step 1: User submits their address.
/// Backend normalises it, calls VicPlan WFS, returns zone/overlay + relevant clauses.
/// </summary>
public record AddressLookupRequestDto(
    [property: JsonPropertyName("address")] string Address
);

/// <summary>
/// Step 2+: Full evaluation context sent after EVERY answer the user provides.
/// The frontend accumulates answers and resends the complete context each time.
/// The backend is stateless — it re-evaluates from scratch on every call.
/// </summary>
public record EvaluationRequestDto(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("normalisedAddress")] string NormalisedAddress,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("zoneCode")] string ZoneCode,
    [property: JsonPropertyName("overlayCodes")] List<string> OverlayCodes,
    [property: JsonPropertyName("buildingTypeSlug")] string BuildingTypeSlug,

    /// <summary>
    /// All answers accumulated so far. Keys are FieldKeys from RuleQuestion.
    /// Values are object — can be double, string, bool, or List&lt;string&gt;.
    /// e.g. { "height_m": 2.4, "location": "front" }
    /// </summary>
    [property: JsonPropertyName("answers")] Dictionary<string, object?> Answers
);

// ─────────────────────────────────────────────
// RESPONSE DTOs (backend → frontend)
// ─────────────────────────────────────────────

/// <summary>
/// Returned from POST /api/permit-checker/address.
/// Gives the frontend everything it needs to render the clause sidebar
/// and proceed to building-type selection.
/// </summary>
public record AddressLookupResponseDto(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("normalisedAddress")] string NormalisedAddress,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("zoneCode")] string ZoneCode,
    [property: JsonPropertyName("zoneDescription")] string ZoneDescription,
    [property: JsonPropertyName("overlayCodes")] List<string> OverlayCodes,

    /// <summary>
    /// Clauses that are relevant for this zone/overlay combination,
    /// regardless of building type. Populates the initial sidebar.
    /// </summary>
    [property: JsonPropertyName("relevantClauses")] List<ClauseDto> RelevantClauses
);

/// <summary>
/// Returned from GET /api/permit-checker/building-types.
/// </summary>
public record BuildingTypeDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("displayOrder")] int DisplayOrder
);

/// <summary>
/// A single planning clause reference. Used in the sidebar and verdict breakdown.
/// </summary>
public record ClauseDto(
    [property: JsonPropertyName("clauseNumber")] string ClauseNumber,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("officialUrl")] string? OfficialUrl
);

/// <summary>
/// A single question the frontend must render and collect an answer for.
/// Returned inside EvaluationResponseDto when status = NeedsMoreInfo.
/// </summary>
public record QuestionDto(
    [property: JsonPropertyName("fieldKey")] string FieldKey,
    [property: JsonPropertyName("questionText")] string QuestionText,
    [property: JsonPropertyName("helpText")] string? HelpText,
    [property: JsonPropertyName("inputType")] string InputType,

    /// <summary>
    /// Populated when InputType is "SingleSelect" or "MultiSelect".
    /// Null for Number and Boolean.
    /// </summary>
    [property: JsonPropertyName("options")] List<QuestionOptionDto>? Options,

    /// <summary>
    /// Populated when InputType is "Number". Null otherwise.
    /// </summary>
    [property: JsonPropertyName("validation")] QuestionValidationDto? Validation,
    [property: JsonPropertyName("displayOrder")] int DisplayOrder
);

public record QuestionOptionDto(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("label")] string Label
);

public record QuestionValidationDto(
    [property: JsonPropertyName("min")] double? Min,
    [property: JsonPropertyName("max")] double? Max,
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("decimalPlaces")] int? DecimalPlaces
);

/// <summary>
/// A single rule that fired and contributed to the verdict.
/// Shown in the verdict breakdown so the user can see exactly which clause
/// and which condition produced the outcome.
/// </summary>
public record TriggeredRuleDto(
    [property: JsonPropertyName("ruleId")] int RuleId,
    [property: JsonPropertyName("outcomeReason")] string OutcomeReason,
    [property: JsonPropertyName("clause")] ClauseDto Clause
);

/// <summary>
/// The main evaluation response. Returned after EVERY call to POST /api/permit-checker/evaluate.
///
/// Status values:
///   "NeedsMoreInfo"  — frontend must render NextQuestions and call evaluate again
///   "Conclusive"     — verdict is final, render the verdict screen
/// </summary>
public record EvaluationResponseDto(
    [property: JsonPropertyName("status")] string Status,    // "NeedsMoreInfo" | "Conclusive"
    [property: JsonPropertyName("outcome")] string? Outcome, // "PermitRequired" | "NoPermitRequired" | "Exempt" | "ReferToCouncil"
    [property: JsonPropertyName("outcomeSummary")] string? OutcomeSummary,

    /// <summary>
    /// Populated when Status = "NeedsMoreInfo".
    /// The frontend renders all questions in this list on the next screen.
    /// Questions are deduplicated by FieldKey so the same question is never shown twice.
    /// </summary>
    [property: JsonPropertyName("nextQuestions")] List<QuestionDto>? NextQuestions,

    /// <summary>
    /// The rules that fired and produced the verdict.
    /// Always populated when Status = "Conclusive".
    /// </summary>
    [property: JsonPropertyName("triggeredRules")] List<TriggeredRuleDto>? TriggeredRules,

    /// <summary>
    /// All clauses in scope at this point in the assessment.
    /// Updated after every evaluate call. Powers the sidebar.
    /// </summary>
    [property: JsonPropertyName("clausesInScope")] List<ClauseDto> ClausesInScope,

    /// <summary>
    /// Audit ID for this assessment. Only set when Status = "Conclusive".
    /// </summary>
    [property: JsonPropertyName("assessmentId")] Guid? AssessmentId
);