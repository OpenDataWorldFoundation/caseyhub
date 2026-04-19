using System.Runtime.CompilerServices;

namespace CaseyHub.Core.Entities;

public class Location
{
    public string  RawAddress {get; private set;} = null!;
    public string? Suburb { get; private set; }
    public string State { get; private set; } = "VIC"; // Hardcoded for Casey Council
    public string? Postcode { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private Location(){}

    public Location(string rawAddress, string? suburb, string? postcode)
    {
        if(string.IsNullOrWhiteSpace(rawAddress)) 
            throw new ArgumentException("Raw Address cannot be empty. ", nameof(rawAddress));
        RawAddress = rawAddress;
        Suburb = suburb;
        Postcode = postcode;
    }
    public void SetCoordinates (double lat, double lng)
    {
        Latitude = lat;
        Longitude = lng;
    }

}