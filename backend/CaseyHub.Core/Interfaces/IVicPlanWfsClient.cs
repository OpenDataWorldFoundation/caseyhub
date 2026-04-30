namespace CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs;
public interface IVicPlanWfsClient
{
    Task<VicPlanZoneResult> GetZoneAndOverlaysAsync (double latitude, double longitude);
}