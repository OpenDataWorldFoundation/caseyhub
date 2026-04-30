namespace CaseyHub.Core.Enums;

public enum RuleType
{
    ZoneOverlayCheck,
    MeasurementThreshold,
    ConditionalCheck,
    Referral
}
public enum RuleOutcome
{
    PermitRequired,
    NoPermitRequired,
    Exempt,
    ReferToCouncil,
    NeedsMoreInfo
}
public enum QuestionInputType
{
    Number,
    SingleSelect,
    MultiSelect,
    Boolean
}
