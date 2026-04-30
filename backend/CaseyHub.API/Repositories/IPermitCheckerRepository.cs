using CaseyHub.Core.Entities;

namespace CaseyHub.API.Repositories;

public interface IPermitCheckerRepository
{
    Task<List<BuildingType>> GetActiveBuildingTypesAsync();
    Task<BuildingType?> GetBuildingTypeBySlugAsync(string slug);
    Task<List<ZoneOverrideRule>> GetZoneOverrideRulesAsync(int buildingTypeId);
    Task<List<PermitRule>> GetPermitRulesWithQuestionsAsync(int buildingTypeId);
    Task<List<PlanningClause>> GetClausesForZoneAsync(string zoneCode);
    Task SaveAssessmentAsync(PermitAssessment assessment);
}