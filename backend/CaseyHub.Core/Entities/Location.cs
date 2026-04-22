using NetTopologySuite.Geometries;
using System.Runtime.CompilerServices;

namespace CaseyHub.Core.Entities;

public class Location
{
    public string  RawAddress {get; private set;} = null!;
    public string? HouseNumber {get; private set;}
    public string? Street {get; private set;}
    public string? Suburb { get; private set; }
    public string? Municipality {get; private set;}
    public string? Postcode { get; private set; }
    public string? State { get; private set; } = "VIC"; // Hardcoded for now
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    public Point? Coordinates {get; private set;}
    private Location(){}

    public Location(string rawAddress)
    { 
        if(string.IsNullOrWhiteSpace(rawAddress)) 
            throw new ArgumentException("Raw Address cannot be empty. ", nameof(rawAddress));
        RawAddress = rawAddress;
    }
    public void SetStructuredLocation(string? houseNumber, string? street, string? suburb, string? municipality, string? postcode, string? state)
    {
        if (!string.IsNullOrWhiteSpace(houseNumber)) HouseNumber = houseNumber.Trim();
        if (!string.IsNullOrWhiteSpace(street)) Street = street.Trim();
        if (!string.IsNullOrWhiteSpace(suburb)) Suburb = suburb.Trim();
        if (!string.IsNullOrWhiteSpace(municipality)) Municipality = municipality.Trim();
        if (!string.IsNullOrWhiteSpace(postcode)) Postcode = postcode.Trim();
        if (!string.IsNullOrWhiteSpace(state)) State = state.Trim();    }
    public void SetCoordinates (double lat, double lng)
    {
        if (lat < -90 || lat > 90) throw new ArgumentOutOfRangeException(nameof(lat), "Latitude must be between -90 and 90.");
        if (lng < -180 || lng > 180) throw new ArgumentOutOfRangeException(nameof(lng), "Longitude must be between -180 and 180.");
        Latitude = lat;
        Longitude = lng;
        Coordinates = new Point (lng, lat) {SRID=4326}; //4326 is a standard for Earth
    }

}