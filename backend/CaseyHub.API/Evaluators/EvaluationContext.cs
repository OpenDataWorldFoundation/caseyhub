namespace CaseyHub.API.Evaluators;

public class EvaluationContext
{
    public string SessionId { get; init; } = null!;
    public string NormalisedAddress { get; init; } = null!;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    
    /// Zone code from VicPlan WFS. e.g. "GRZ1", "UGZ14", "NRZ3"
    public string ZoneCode { get; init; } = null!;

    /// Overlay codes from VicPlan WFS. e.g. ["HO123", "BMO", "DDO5"]
    public List<string> OverlayCodes { get; init; } = new();

    public string BuildingTypeSlug { get; init; } = null!;

    /// <summary>
    /// All user answers accumulated so far.
    /// Keys are FieldKeys from RuleQuestion. Values are the raw deserialized objects.
    /// The evaluator reads from this dictionary to test rule conditions.
    /// </summary>
    public Dictionary<string, object?> Answers { get; init; } = new();
}