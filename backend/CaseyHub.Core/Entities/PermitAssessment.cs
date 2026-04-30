using CaseyHub.Core.Enums;

namespace CaseyHub.Core.Entities;

/// <summary>
/*********** HISTORY TABLE ***********/
/// Immutable audit record of a completed permit assessment.
/// Written once when the evaluation reaches a conclusive verdict.
/// Never updated — a new row is created for each assessment.
///
/// This enables:
///   - Analytics: "how many fence assessments resulted in permit required in Casey last month?"
///   - Evidence: a timestamped record of the recommendation given to a user
///   - Debugging: full answer payload for reproducing any verdict
/// </summary>
public class PermitAssessment
{
    public Guid Id { get; private set; }

    /// The normalised address string returned by Nominatim.
    public string NormalisedAddress { get; private set; } = null!;

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    /// Zone code returned by VicPlan WFS. e.g. "GRZ1"
    public string ZoneCode { get; private set; } = null!;

    /// Comma-separated overlay codes. e.g. "BMO,DDO5"
    /// Stored as a simple string — this is an audit log, not a query target.
    public string OverlayCodes { get; private set; } = null!;

    public string BuildingTypeSlug { get; private set; } = null!;

    /// Full JSON snapshot of all user answers at the time of verdict.
    /// e.g. { "height_m": 2.4, "location": "front" }
    public string AnswersJson { get; private set; } = null!;

    public RuleOutcome Outcome { get; private set; }

    /// The final human-readable verdict reason.
    public string OutcomeReason { get; private set; } = null!;

    /// Comma-separated clause numbers that were cited in the verdict.
    /// e.g. "62.02-2,54.06-2"
    public string TriggeredClauseNumbers { get; private set; } = null!;

    public DateTime AssessedAtUtc { get; private set; }

    /// Optional — null for anonymous users.
    public Guid? UserId { get; private set; }

    private PermitAssessment() { }

    public PermitAssessment(
        string normalisedAddress,
        double latitude,
        double longitude,
        string zoneCode,
        string overlayCodes,
        string buildingTypeSlug,
        string answersJson,
        RuleOutcome outcome,
        string outcomeReason,
        string triggeredClauseNumbers,
        Guid? userId)
    {
        Id = Guid.NewGuid();
        NormalisedAddress = normalisedAddress;
        Latitude = latitude;
        Longitude = longitude;
        ZoneCode = zoneCode;
        OverlayCodes = overlayCodes;
        BuildingTypeSlug = buildingTypeSlug;
        AnswersJson = answersJson;
        Outcome = outcome;
        OutcomeReason = outcomeReason;
        TriggeredClauseNumbers = triggeredClauseNumbers;
        AssessedAtUtc = DateTime.UtcNow;
        UserId = userId;
    }
}