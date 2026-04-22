using CaseyHub.Models.DTOs.Internal.Permit;

namespace CaseyHub.API.Services;

public interface IPermitService
{
    Task<PermitDto?> GetPermitByAppNumberAsync (string applicationNumber);
    Task AddPermitByAppNumberToDBAsync (string applicationNumber);
    Task EnrichSaveAllPermitsAsync();
    Task SyncPermitsAsync();
    Task<List<PermitDto>> GetPermitsNearAddressAsync(string address, int radius);
}