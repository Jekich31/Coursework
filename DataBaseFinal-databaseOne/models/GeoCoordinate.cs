namespace ProjectFIN.models;

public class Coordinate
{
    public int Id { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double? HomeZoneLatitude { get; set; }
    public double? HomeZoneLongitude { get; set; }
    public double AllowedRadius { get; set; }
    public string? VehicleVin { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Coordinate() { }

    public Coordinate(double lat, double lng, double? homeLat = null, double? homeLng = null, double allowedRadius = 0)
    {
        Latitude = lat;
        Longitude = lng;
        HomeZoneLatitude = homeLat;
        HomeZoneLongitude = homeLng;
        AllowedRadius = allowedRadius;
    }
    public override string ToString()
    {
        return $"{Latitude:F4}, {Longitude:F4}";
    }
}