using System;
using ProjectFIN.models;

namespace ProjectFIN.models;

public class ElectricCar : Vehicle
{
    public double BatteryCapacity { get; set; }
    public double CurrentCharge { get; set; }
    public ElectricCar() : base() { }

    public ElectricCar(string vin, string brand, string model, double startLat, double startLng, double batteryCapacity)
    : base(vin, brand, model, startLat, startLng, capacity: batteryCapacity, currentLevel: batteryCapacity)
    {
        BatteryCapacity = batteryCapacity;
        CurrentCharge = batteryCapacity;
    }

    public override string GetResourceStatus()
    {
        return $"Battery: {CurrentCharge:F1} / {BatteryCapacity} kWh ({(CurrentCharge / BatteryCapacity) * 100:F0}%)";
    }
}