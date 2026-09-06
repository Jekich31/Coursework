using ProjectFIN.InterFaces;
using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ProjectFIN.models;

[JsonDerivedType(typeof(ElectricCar), typeDiscriminator: "EV")]
[JsonDerivedType(typeof(GasolineCar), typeDiscriminator: "Gas")]
public abstract class Vehicle : IRemoteControllable
{
    [Key]
    public string Vin { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public string Model { get; init; } = null!;
    public EngineState Engine { get; protected set; } = EngineState.Stopped;
    public DoorState Doors { get; protected set; } = DoorState.Locked;
    public virtual Coordinate LocationData { get; set; } = null!;

    [JsonIgnore]
    private IVehicleState _state = null!;

    public event Action<string>? OnGeofenceViolation;
    protected Vehicle()
    {
        InitializeState();
    }
    public Vehicle(string vin, string brand, string model, double startLat, double startLng)
    {
        Vin = vin;
        Brand = brand;
        Model = model;
        LocationData = new Coordinate(startLat, startLng) { VehicleVin = vin };
        InitializeState();
    }

    private void InitializeState()
    {
        if (Engine == EngineState.Running)
        {
            _state = new RunningState();
        }
        else
        {
            _state = new StoppedState();
        }
    }

    public void SetState(IVehicleState state)
    {
        _state = state;
    }

    public void SetEngineState(EngineState engineState)
    {
        Engine = engineState;

        if (Engine == EngineState.Running)
            _state = new RunningState();
        else
            _state = new StoppedState();
    }

    public void SetDoorState(DoorState doorState)
    {
        Doors = doorState;
    }
    public void SetGeofence(double lat, double lng, double radius)
    {
        if (LocationData == null)
        {
            LocationData = new Coordinate { VehicleVin = Vin };
        }
        LocationData.HomeZoneLatitude = lat;
        LocationData.HomeZoneLongitude = lng;
        LocationData.AllowedRadius = radius;
    }
    public void UpdateLocation(double lat, double lng)
    {
        if (LocationData == null)
        {
            LocationData = new Coordinate(lat, lng) { VehicleVin = Vin };
        }
        else
        {
            LocationData.Latitude = lat;
            LocationData.Longitude = lng;
        }

        if (LocationData.HomeZoneLatitude.HasValue && LocationData.HomeZoneLongitude.HasValue)
        {
            double distance = CalculateDistance(
                LocationData.Latitude, LocationData.Longitude,
                LocationData.HomeZoneLatitude.Value, LocationData.HomeZoneLongitude.Value
            );

            if (distance > LocationData.AllowedRadius)
            {
                OnGeofenceViolation?.Invoke($"ALARM! Vehicle {Brand} {Model} (VIN: {Vin}) left the safe zone! Current distance: {distance:F1} meters out of {LocationData.AllowedRadius}m limit.");
            }
        }
    }
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3;
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    public void LockDoors()
    {
        _state.LockDoors(this);
    }

    public void UnlockDoors()
    {
        _state.UnlockDoors(this);
    }

    public virtual void StartEngine()
    {
        _state.StartEngine(this);
        _state = new RunningState();
    }

    public void StopEngine()
    {
        _state.StopEngine(this);
        _state = new StoppedState();
    }

    public abstract string GetResourceStatus();
}