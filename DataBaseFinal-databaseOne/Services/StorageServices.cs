using ProjectFIN.models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ProjectFIN.services;

public static class StorageService
{
    public static List<Vehicle> LoadVehicles()
    {
        try
        {
            using var db = new AppDbContext();
            var vehicles = db.Vehicles.Include(v => v.LocationData).ToList();

            if (vehicles != null)
            {
                foreach (var vehicle in vehicles)
                {
                    if (vehicle.Engine == EngineState.Running)
                        vehicle.SetState(new RunningState());
                    else
                        vehicle.SetState(new StoppedState());
                }
            }

            return vehicles ?? new List<Vehicle>();
        }
        catch
        {
            return new List<Vehicle>();
        }
    }
    public static void SaveVehicles(List<Vehicle> vehicles)
    {
        try
        {
            using var db = new AppDbContext();

            foreach (var vehicle in vehicles)
            {
                var existing = db.Vehicles
                    .Include(v => v.LocationData)
                    .FirstOrDefault(v => v.Vin == vehicle.Vin);

                if (existing == null)
                {
                    db.Vehicles.Add(vehicle);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(vehicle);
                    if (vehicle.LocationData != null)
                    {
                        if (existing.LocationData == null)
                        {
                            existing.LocationData = new Coordinate
                            {
                                VehicleVin = vehicle.Vin,
                                Latitude = vehicle.LocationData.Latitude,
                                Longitude = vehicle.LocationData.Longitude,
                                HomeZoneLatitude = vehicle.LocationData.HomeZoneLatitude,
                                HomeZoneLongitude = vehicle.LocationData.HomeZoneLongitude,
                                AllowedRadius = vehicle.LocationData.AllowedRadius
                            };
                        }
                        else
                        {
                            db.Entry(existing.LocationData).CurrentValues.SetValues(vehicle.LocationData);
                        }
                    }
                }
            }

            db.SaveChanges();
        }
        catch
        {
        }
    }
}