using System;
using ProjectFIN.models;

namespace ProjectFIN.models;

public class GasolineCar : Vehicle
{
    public double FuelTankCapacity { get; set; }
    public double CurrentFuel { get; set; }

    public GasolineCar() : base() { }
    public GasolineCar(string vin, string brand, string model, double startLat, double startLng, double fuelTankCapacity)
     : base(vin, brand, model, startLat, startLng, capacity: fuelTankCapacity, currentLevel: fuelTankCapacity)
    {
        FuelTankCapacity = fuelTankCapacity;
        CurrentFuel = fuelTankCapacity;
    }

    public override string GetResourceStatus()
    {
        return $"Fuel: {CurrentFuel:F1} / {FuelTankCapacity} L ({(CurrentFuel / FuelTankCapacity) * 100:F0}%)";
    }
}