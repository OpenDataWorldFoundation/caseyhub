namespace CaseyHub.Core.Entities;


public class PlanningClause
{
    public int Id {get; private set;}
    public string ClauseNumber { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Summary { get; private set; }
    public string? OfficialUrl { get; private set; }
    private PlanningClause(){}

    public PlanningClause (string clauseNumber, string title, string? summary, string? officialUrl)
    {
        ClauseNumber = clauseNumber;
        Title = title;
        Summary = summary;
        OfficialUrl = officialUrl;
    }
}