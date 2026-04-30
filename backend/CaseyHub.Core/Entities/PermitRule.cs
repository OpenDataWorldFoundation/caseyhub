using CaseyHub.Core.Enums;

namespace CaseyHub.Core.Entities;

/// <summary>
/// A single evaluable rule in the permit decision engine.
///
/// Rules are evaluated in ascending Priority order after ZoneOverrideRules pass.
/// Each rule has a JSONB TriggerContext that the evaluator interprets to decide
/// whether the rule's condition is satisfied.
///
/// TriggerContext JSONB schemas:
///
/// 1. Measurement threshold:
///    { "field": "height_m", "operator": "gt", "value": 2.0 }
///    { "field": "height_m", "operator": "gt", "value": 1.5, "when": { "location": "front" } }
///
/// 2. Exact field match:
///    { "field": "location", "operator": "eq", "value": "front" }
///
/// 3. Compound (all conditions must be true):
///    { "all": [
///        { "field": "location", "operator": "eq", "value": "front" },
///        { "field": "height_m", "operator": "gt", "value": 1.5 }
///      ]
///    }
///
/// 4. Compound (any condition must be true):
///    { "any": [
///        { "field": "height_m", "operator": "gt", "value": 2.0 },
///        { "field": "location", "operator": "eq", "value": "front" }
///      ]
///    }
///
/// 5. Zone/overlay check (against VicPlan data):
///    { "zone_any": ["GRZ", "NRZ", "RGZ"] }
///    { "overlay_any": ["BMO"] }
///    { "zone_not_any": ["TRZ2"] }
///
/// Supported operators: "gt", "gte", "lt", "lte", "eq", "neq"
/// </summary>
public class PermitRule
{
    public int Id { get; private set; }

    public int BuildingTypeId { get; private set; }
    public BuildingType BuildingType { get; private set; } = null!;

    public RuleType RuleType { get; private set; }

    /// Lower number = evaluated first.
    /// Zone/overlay checks: 1–10. Measurement rules: 20–50. Compound rules: 51–100.
    public int Priority { get; private set; }

    public string TriggerContextJson { get; private set; } = null!;

    public RuleOutcome Outcome { get; private set; }

    public string OutcomeReason { get; private set; } = null!;

    public int PlanningClauseId { get; private set; }
    public PlanningClause PlanningClause { get; private set; } = null!;

    public bool ShortCircuitOnMatch { get; private set; }

    public bool IsActive { get; private set; } = true;

    // Navigation — the questions required before this rule can be evaluated
    public ICollection<RuleQuestion> Questions { get; private set; } = new List<RuleQuestion>();

    private PermitRule() { }

    public PermitRule(
        int buildingTypeId,
        RuleType ruleType,
        int priority,
        string triggerContextJson,
        RuleOutcome outcome,
        string outcomeReason,
        int planningClauseId,
        bool shortCircuitOnMatch = true)
    {
        BuildingTypeId = buildingTypeId;
        RuleType = ruleType;
        Priority = priority;
        TriggerContextJson = triggerContextJson;
        Outcome = outcome;
        OutcomeReason = outcomeReason;
        PlanningClauseId = planningClauseId;
        ShortCircuitOnMatch = shortCircuitOnMatch;
        IsActive = true;
    }
}