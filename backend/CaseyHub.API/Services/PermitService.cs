using CaseyHub.API.Services;
using CaseyHub.Core.Entities;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.Internal;

public class PermitService(ICouncilDataClient councilDataClient) : IPermitService
{
    public async Task<PermitDto?> GetPermitByAppNumberAsync (string applicationNumber)
    {
        Permit? permitEntity = await councilDataClient.FetchPermitFromAppNumberAsync(applicationNumber);
        if(permitEntity == null)
        {
            return null;
        }
        PermitDto? permitDto = new PermitDto(
            ApplicationNumber: permitEntity.ApplicationNumber,
            ApplicationCategory: permitEntity.ApplicationCategory,
            Description: permitEntity.Description,
            Status: permitEntity.Status,
            StageDecision: permitEntity.StageDecision,
            Address: permitEntity.Address,
            LodgedDate: permitEntity.LodgedDate,
            DecisionDate: permitEntity.DecisionDate
        );
        return permitDto;
    }
}