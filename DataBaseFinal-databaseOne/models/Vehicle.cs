using ProjectFIN.InterFaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ProjectFIN.models;

[JsonDerivedType(typeof(ElectricCar), typeDiscriminator: "EV")]
[JsonDerivedType(typeof(GasolineCar), typeDiscriminator: "Gas")]
public abstract class Vehicle : IRemoteControllable, INotifyPropertyChanged
{
    [Key]
    public string Vin { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public string Model { get; init; } = null!;

    private EngineState _engine = EngineState.Stopped;
    public EngineState Engine
    {
        get => _engine;
        protected set { _engine = value; OnPropertyChanged(); }
    }

    private DoorState _doors = DoorState.Locked;
    public DoorState Doors
    {
        get => _doors;
        protected set { _doors = value; OnPropertyChanged(); }
    }

    private Coordinate _locationData = null!;
    public virtual Coordinate LocationData
    {
        get => _locationData;
        set { _locationData = value; OnPropertyChanged(); }
    }

    public double Capacity { get; set; } = 100;

    private double _currentLevel = 50;
    public double CurrentLevel
    {
        get => _currentLevel;
        set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(CapacityInfo)); }
    }

    [JsonIgnore]
    public string CapacityInfo => $"{CurrentLevel:F0} / {Capacity:F0}";

    [JsonIgnore]
    private IVehicleState _state = null!;

    public event Action<string>? OnGeofenceViolation;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected Vehicle()
    {
        _state = new StoppedState();
    }

    public Vehicle(string vin, string brand, string model, double startLat, double startLng, double capacity = 100, double currentLevel = 50)
    {
        Vin = vin;
        Brand = brand;
        Model = model;
        Capacity = capacity;
        CurrentLevel = currentLevel;
        LocationData = new Coordinate(startLat, startLng) { VehicleVin = vin };
        _state = new StoppedState();
    }

    public void SetState(IVehicleState state)
    {
        _state = state;
    }

    public void SetEngineState(EngineState engineState)
    {
        Engine = engineState;
        _state = Engine == EngineState.Running ? new RunningState() : new StoppedState();
    }

    public void SetDoorState(DoorState doorState)
    {
        Doors = doorState;
    }

    public void LockDoors() => _state.LockDoors(this);
    public void UnlockDoors() => _state.UnlockDoors(this);

    public virtual void StartEngine()
    {
        if (CurrentLevel <= 0)
            throw new Exception("Неможливо запустити двигун: паливо/заряд на нулі!");

        _state.StartEngine(this);
    }

    public void StopEngine()
    {
        _state.StopEngine(this);
    }

    public void ConsumeResource(double amount = 1.0)
    {
        if (Engine == EngineState.Running)
        {
            CurrentLevel -= amount;
            if (CurrentLevel <= 0)
            {
                CurrentLevel = 0;
                StopEngine();
            }
        }
    }

    public void UpdateLocation(double lat, double lng)
    {
        if (LocationData == null)
            LocationData = new Coordinate(lat, lng) { VehicleVin = Vin };
        else
        {
            LocationData.Latitude = lat;
            LocationData.Longitude = lng;
        }
    }

    public abstract string GetResourceStatus();
}