namespace CaseyHub.Core.Entities;

public class Permit
{
    public string ApplicationNumber { get; private set; }
    public string? ApplicationCategory { get; private set; }
    public string? Description { get; private set; }
    public string? Status { get; private set; }
    public string? DecisionStage { get; private set; }
    public string? Decision {get; private set;}
    public string? ServiceArea {get; private set;}
    public Location? Location { get; private set; }
    public DateTime? LodgedDate { get; private set; }
    public DateTime? DecisionDate { get; private set; }
    public DateTime? AdvertiseCommencedDate { get; private set; }
    public DateTime? AdvertiseCompletedDate { get; private set; }

    private Permit()
    {
        ApplicationNumber = string.Empty;
    }

    public Permit(
        string applicationNumber, 
        string? applicationCategory, 
        string? description, 
        string? status, 
        string? decisionStage, 
        string? decision,
        string serviceArea,
        Location? location, 
        DateTime? lodgedDate, 
        DateTime? decisionDate,
        DateTime? advertiseCommencedDate,
        DateTime? advertiseCompletedDate
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
        DecisionStage = decisionStage;
        Decision = decision;
        ServiceArea = serviceArea;
        Location = location;
        LodgedDate = lodgedDate;
        DecisionDate = decisionDate;
        AdvertiseCommencedDate = advertiseCommencedDate;
        AdvertiseCompletedDate = advertiseCompletedDate;
    }

    public void UpdateDetails(
        string? applicationCategory, 
        string? description, 
        string? status, 
        string? decisionStage, 
        string? decision,
        string? serviceArea,
        DateTime? lodgedDate, 
        DateTime? decisionDate,
        DateTime? advertiseCommencedDate,
        DateTime? advertiseCompletedDate)
    {
        ApplicationCategory = applicationCategory;
        Description = description;
        Status = status;
        DecisionStage = decisionStage;
        Decision = decision;
        ServiceArea = serviceArea;
        LodgedDate = lodgedDate;
        DecisionDate = decisionDate;
        AdvertiseCommencedDate = advertiseCommencedDate;
        AdvertiseCompletedDate = advertiseCompletedDate;
    }
}
