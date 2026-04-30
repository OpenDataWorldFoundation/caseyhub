using CaseyHub.API.Data;
using CaseyHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseyHub.API.Repositories;

public class PermitCheckerRepository(CaseyHubDbContext db) : IPermitCheckerRepository
{
    public Task<List<BuildingType>> GetActiveBuildingTypesAsync() =>
        db.BuildingTypes
          .Where(b => b.IsActive)
          .OrderBy(b => b.DisplayOrder)
          .ToListAsync();

    public Task<BuildingType?> GetBuildingTypeBySlugAsync(string slug) =>
        db.BuildingTypes
          .FirstOrDefaultAsync(b => b.Slug == slug && b.IsActive);

    /// <summary>
    /// Returns all active ZoneOverrideRules for a building type, with their clauses eager-loaded.
    /// Used in the first stage of evaluation (before any user questions are asked).
    /// </summary>
    public Task<List<ZoneOverrideRule>> GetZoneOverrideRulesAsync(int buildingTypeId) =>
        db.ZoneOverrideRules
          .Include(r => r.PlanningClause)
          .Where(r => r.BuildingTypeId == buildingTypeId && r.IsActive)
          .ToListAsync();

    /// <summary>
    /// Returns all active PermitRules for a building type, ordered by priority,
    /// with their questions and clauses eager-loaded.
    /// </summary>
    public Task<List<PermitRule>> GetPermitRulesWithQuestionsAsync(int buildingTypeId) =>
        db.PermitRules
          .Include(r => r.Questions)
          .Include(r => r.PlanningClause)
          .Where(r => r.BuildingTypeId == buildingTypeId && r.IsActive)
          .OrderBy(r => r.Priority)
          .ToListAsync();

    /// <summary>
    /// Returns clauses that are broadly relevant to the given zone code.
    /// Used to populate the initial clause sidebar after address lookup.
    /// This is a heuristic — it returns all clauses we know are relevant to
    /// residential or growth zones in Casey.
    /// </summary>
    public async Task<List<PlanningClause>> GetClausesForZoneAsync(string zoneCode)
    {
        // Always-relevant clauses regardless of zone
        var alwaysRelevant = new[] { "62.02-2" };

        // Zone-specific mappings
        var zoneClauseMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["GRZ"]  = new[] { "32.08", "54.06-2", "62.02-2" },
            ["NRZ"]  = new[] { "32.09", "54.06-2", "62.02-2" },
            ["RGZ"]  = new[] { "32.07", "54.06-2", "62.02-2" },
            ["UGZ"]  = new[] { "37.07", "62.02-2" },
            ["ACZ"]  = new[] { "37.08", "54.06-2", "62.02-2" },
            ["MUZ"]  = new[] { "54.06-2", "62.02-2" },
            ["TZ"]   = new[] { "54.06-2", "62.02-2" },
        };

        // Find the matching zone prefix
        var matchedClauses = alwaysRelevant.ToList();
        foreach (var kvp in zoneClauseMap)
        {
            if (zoneCode.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                matchedClauses.AddRange(kvp.Value);
                break;
            }
        }

        var distinctClauseNumbers = matchedClauses.Distinct().ToList();

        return await db.PlanningClauses
            .Where(c => distinctClauseNumbers.Contains(c.ClauseNumber))
            .ToListAsync();
    }

    public async Task SaveAssessmentAsync(PermitAssessment assessment)
    {
        await db.PermitAssessments.AddAsync(assessment);
        await db.SaveChangesAsync();
    }
}