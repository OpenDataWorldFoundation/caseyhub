using CaseyHub.Core.Enums;

namespace CaseyHub.Core.Entities;

/// <summary>
/// A question the frontend must ask the user before the associated PermitRule can be evaluated.
/// One rule can have multiple questions. The evaluator collects ALL unanswered questions across
/// ALL rules and returns them together as a batch — the frontend renders one screen per batch.
///
/// OptionsJson (for SingleSelect / MultiSelect) format:
/// {
///   "choices": [
///     { "value": "front", "label": "Front (within 3m of the street)" },
///     { "value": "side_rear", "label": "Side or rear" }
///   ]
/// }
///
/// ValidationJson (for Number inputs) format:
/// { "min": 0, "max": 20, "unit": "m", "decimalPlaces": 2 }
/// </summary>
public class RuleQuestion
{
    public int Id { get; private set; }

    public int PermitRuleId { get; private set; }
    public PermitRule PermitRule { get; private set; } = null!;

    /// <summary>
    /// The key used in EvaluationContext.Answers dictionary.
    /// e.g. "height_m", "location", "is_front_within_3m", "lot_size_sqm"
    /// Must be unique per BuildingType (not globally) — the same key can appear in
    /// multiple rules for the same building type, but each asks the same question
    /// so duplicates are deduplicated before sending to the frontend.
    /// </summary>
    public string FieldKey { get; private set; } = null!;

    /// The question text displayed to the user. e.g. "What is the proposed fence height?"
    public string QuestionText { get; private set; } = null!;

    /// Optional helper text shown below the input. e.g. "Measure from natural ground level."
    public string? HelpText { get; private set; }

    public QuestionInputType InputType { get; private set; }

    /// JSONB. Required when InputType is SingleSelect or MultiSelect.
    /// Null for Number and Boolean inputs.
    public string? OptionsJson { get; private set; }

    /// JSONB. Required when InputType is Number. Null otherwise.
    public string? ValidationJson { get; private set; }

    /// Controls render order when multiple questions are shown on the same screen.
    public int DisplayOrder { get; private set; }

    private RuleQuestion() { }

    public RuleQuestion(
        int permitRuleId,
        string fieldKey,
        string questionText,
        string? helpText,
        QuestionInputType inputType,
        string? optionsJson,
        string? validationJson,
        int displayOrder)
    {
        PermitRuleId = permitRuleId;
        FieldKey = fieldKey;
        QuestionText = questionText;
        HelpText = helpText;
        InputType = inputType;
        OptionsJson = optionsJson;
        ValidationJson = validationJson;
        DisplayOrder = displayOrder;
    }
}