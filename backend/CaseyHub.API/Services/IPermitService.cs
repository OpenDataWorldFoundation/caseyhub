using CaseyHub.Models.DTOs.Internal;

namespace CaseyHub.API.Services;

public interface IPermitService
{
    Task<PermitDto?> GetPermitByAppNumberAsync (string applicationNumber);
}