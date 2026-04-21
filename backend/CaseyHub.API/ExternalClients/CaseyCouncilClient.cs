using CaseyHub.Core.Entities;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.External;

namespace CaseyHub.API.ExternalClients;

public class CaseyCouncilClient (HttpClient httpClient) : ICouncilDataClient
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
                lodgedDate: DateTime.TryParse(externalDto.LodgedDate, out var lodged)? lodged:null,
                decisionDate: DateTime.TryParse(externalDto.DecisionDate, out var decision) ? decision : null
            );

            return permit;

        }catch(HttpRequestException ex)
        {
            Console.WriteLine("Error Fetching Data from casey API", ex.Message);
            return null;
        }
    }
}