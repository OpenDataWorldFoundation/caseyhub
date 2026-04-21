using CaseyHub.Core.Entities;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.External;

namespace CaseyHub.API.ExternalClients;

public class CaseyCouncilClient (HttpClient httpClient, ILogger<CaseyCouncilClient> logger) : ICouncilDataClient
{
    public async Task<Permit?> FetchPermitFromAppNumberAsync (string applicationNumber)
    {
        try
        {
            var queryUrl = $"planning-permit-applications-register-only/records?where=application_number='{applicationNumber}'&limit=1";
            CaseyPermitResponse? response = await httpClient.GetFromJsonAsync<CaseyPermitResponse>(queryUrl);
            CaseyPermitDto? externalDto = response?.Results?.FirstOrDefault();
            if(externalDto == null)
            {   
                return null;
            }
            Location? permitLocation = null;

            if (!string.IsNullOrWhiteSpace(externalDto.PlanPermitAddress))
            {
                permitLocation = new Location(
                    rawAddress: externalDto.PlanPermitAddress
                );
            }

            var permit = new Permit(
                applicationNumber: externalDto.ApplicationNumber,
                applicationCategory: externalDto.ApplicationCategory,
                description: externalDto.Description,
                status: externalDto.Status,
                stageDecision: externalDto.StageDecision,
                location: permitLocation,
                lodgedDate: DateTime.TryParse(externalDto.LodgedDate, out var lodged)? DateTime.SpecifyKind(lodged, DateTimeKind.Utc) :null,
                decisionDate: DateTime.TryParse(externalDto.DecisionDate, out var decision) ? DateTime.SpecifyKind(decision, DateTimeKind.Utc) : null
            );

            return permit;

        }catch(HttpRequestException ex)
        {
            Console.WriteLine("Error Fetching Data from casey API", ex.Message);
            return null;
        }
    }

    public async Task<List<Permit>> GetAllPermits()
    {
        try
        {
            logger.LogInformation("Making the call to Planning Permit Dataset, with /exports/json");
            var queryUrl = $"planning-permit-applications-register-only/exports/json";
            //Not wrapping in CaseyResponseDTO as export option sends the result array, doesn't send {totalCount: , results:[] }. only results:[]
            List<CaseyPermitDto>? allPermitsDTO = await httpClient.GetFromJsonAsync<List<CaseyPermitDto>>(queryUrl);
            logger.LogInformation("Casey Permit Response received. Number of Permits Received: {count} ", allPermitsDTO?.Count);
            if(allPermitsDTO == null)
            {
                logger.LogCritical("All permits returned null. IS THE DATASET CORRECT?");
                return new List<Permit>();
            }
            var permits = new List<Permit>();

            foreach (var onePermit in allPermitsDTO)
            {
                Location? permitLocation = null;
                if (!string.IsNullOrWhiteSpace(onePermit.PlanPermitAddress))
                {
                    permitLocation = new Location(rawAddress: onePermit.PlanPermitAddress);
                }
                var permit = new Permit(
                    applicationNumber: onePermit.ApplicationNumber,
                    applicationCategory: onePermit.ApplicationCategory,
                    description: onePermit.Description,
                    status: onePermit.Status,
                    stageDecision: onePermit.StageDecision,
                    location: permitLocation,
                    lodgedDate: DateTime.TryParse(onePermit.LodgedDate, out var lodged)? DateTime.SpecifyKind(lodged, DateTimeKind.Utc) :null,
                    decisionDate: DateTime.TryParse(onePermit.DecisionDate, out var decision) ? DateTime.SpecifyKind(decision, DateTimeKind.Utc) : null
                );
                permits.Add(permit);
            };
            logger.LogInformation("Succesfully mapped {Count} permits", allPermitsDTO.Count);
            return permits;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all permits.");
            return new List<Permit>();
        }
    }
}