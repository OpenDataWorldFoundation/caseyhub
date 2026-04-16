namespace CaseyHub.Core.Entities;

public class Permit
{
    public string ApplicationNumber { get; private set; }
    public string? ApplicationCategory { get; private set; }
    public string? Description { get; private set; }
    public string? Status { get; private set; }
    public string? StageDecision { get; private set; }
    public string? Address { get; private set; }
    public DateTime? LodgedDate { get; private set; }
    public DateTime? DecisionDate { get; private set; }

    public Permit(
        string applicationNumber, 
        string? applicationCategory, 
        string? description, 
        string? status, 
        string? stageDecision, 
        string? address, 
        DateTime? lodgedDate, 
        DateTime? decisionDate
    )
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
        {
            throw new ArgumentException("Application number is required to instantiate a Permit.", nameof(applicationNumber));
        }

        ApplicationNumber = applicationNumber;
        ApplicationCategory = applicationCategory;
        Description = description;
        Status = status;
        StageDecision = stageDecision;
        Address = address;
        LodgedDate = lodgedDate;
        DecisionDate = decisionDate;
    }
}