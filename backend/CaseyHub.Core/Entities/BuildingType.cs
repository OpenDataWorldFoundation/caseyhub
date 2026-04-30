namespace CaseyHub.Core.Entities;

public class BuildingType
{
    public int Id {get; private set;}
    public string Slug {get; private set;} = null!;
    public string DisplayName {get; private set;} = null!;
    public string? Description {get; private set;}
    public int DisplayOrder {get; private set;} //for frontend
    public bool IsActive {get; private set;}
    public ICollection<PermitRule> PermitRules {get; private set;} = new List<PermitRule>();
    public ICollection<ZoneOverrideRule> ZoneOverrideRules {get; private set;} = new List<ZoneOverrideRule>();

    private BuildingType(){}

    public BuildingType(string slug, string displayName, string? description, int displayOrder)
    {
        Slug = slug;
        DisplayName = displayName;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = true;
    }
}