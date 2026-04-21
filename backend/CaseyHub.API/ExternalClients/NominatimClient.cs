using System.Text.Json;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.External;

namespace CaseyHub.API.ExternalClients;

public class NominatimClient (HttpClient httpClient, IConfiguration config): INominatimClient
{
    public async Task<GeoCodeResponse?> EnrichAddressAsync(string rawAddress, bool usePrivateServer)
    {
        string baseUrl = usePrivateServer ? config["Nominatim:PrivateUrl"] ?? "" : "https://nominatim.openstreetmap.org/search";
        if (!usePrivateServer)
        {
            await Task.Delay(1000);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CaseyHub-Backend-Sync");
        };

        var requestURI = $"{baseUrl}?q={Uri.EscapeDataString(rawAddress)}&format=json&addressdetails=1&limit=1";

        List<GeoCodeResponse>? response = await httpClient.GetFromJsonAsync<List<GeoCodeResponse>>(requestURI);
        
        return response?.FirstOrDefault();
    }
}