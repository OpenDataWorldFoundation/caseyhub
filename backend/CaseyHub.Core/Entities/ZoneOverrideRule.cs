using CaseyHub.Core.Enums;

namespace CaseyHub.Core.Entities;

public class ZoneOverrideRule
{
        public int Id { get; private set; }
 
    public int BuildingTypeId { get; private set; }
    public BuildingType BuildingType { get; private set; } = null!;
    public string ZoneOrOverlayCode { get; private set; } = null!;
    
    /// If true, the code is matched as a prefix (e.g. "UGZ" matches "UGZ1", "UGZ14").
    /// If false, exact match only.
    public bool PrefixMatch { get; private set; }
    public RuleOutcome Outcome { get; private set; }
    public string OutcomeReason { get; private set; } = null!; //Human Readable
    public int PlanningClauseId { get; private set; }
    public PlanningClause PlanningClause { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    private ZoneOverrideRule() { }
    public ZoneOverrideRule(
        int buildingTypeId,
        string zoneOrOverlayCode,
        bool prefixMatch,
        RuleOutcome outcome,
        string outcomeReason,
        int planningClauseId)
    {
        BuildingTypeId = buildingTypeId;
        ZoneOrOverlayCode = zoneOrOverlayCode;
        PrefixMatch = prefixMatch;
        Outcome = outcome;
        OutcomeReason = outcomeReason;
        PlanningClauseId = planningClauseId;
        IsActive = true;
    }
}
