using CaseyHub.Models.DTOs.External;

namespace CaseyHub.Core.Interfaces;
public interface INominatimClient
{   
    Task<GeoCodeResponse?> EnrichAddressAsync (string rawAddress, bool usePrivateServer);
}