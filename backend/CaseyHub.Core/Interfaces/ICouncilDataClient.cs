using CaseyHub.Core.Entities;

namespace CaseyHub.Core.Interfaces;

public interface ICouncilDataClient
{
    Task<Permit?> FetchPermitFromAppNumberAsync (string applicationNumber);
}