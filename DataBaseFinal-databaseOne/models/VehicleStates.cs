using System;

namespace ProjectFIN.models;

public interface IVehicleState
{
    void StartEngine(Vehicle vehicle);
    void StopEngine(Vehicle vehicle);
    void LockDoors(Vehicle vehicle);
    void UnlockDoors(Vehicle vehicle);
}

public class StoppedState : IVehicleState
{
    public void StartEngine(Vehicle vehicle)
    {
        vehicle.SetEngineState(EngineState.Running);
        vehicle.SetState(new RunningState());
    }

    public void StopEngine(Vehicle vehicle)
    {
        vehicle.SetEngineState(EngineState.Stopped);
        vehicle.SetState(new StoppedState());
    }

    public void LockDoors(Vehicle vehicle)
    {
        if (vehicle.Doors == DoorState.Locked)
        {
            throw new Exception("Doors are already locked and secured.");
        }
        vehicle.SetDoorState(DoorState.Locked);
    }

    public void UnlockDoors(Vehicle vehicle)
    {
        if (vehicle.Doors == DoorState.Unlocked)
        {
            throw new Exception("Doors are already unlocked.");
        }
        vehicle.SetDoorState(DoorState.Unlocked);
    }
}

public class RunningState : IVehicleState
{
    public void StartEngine(Vehicle vehicle)
    {
        throw new Exception("The engine is already running!");
    }

    public void StopEngine(Vehicle vehicle)
    {
        vehicle.SetEngineState(EngineState.Stopped);
        vehicle.SetState(new StoppedState());
    }

    public void LockDoors(Vehicle vehicle)
    {
        if (vehicle.Doors == DoorState.Locked)
        {
            throw new Exception("Doors are already locked and secured.");
        }
        vehicle.SetDoorState(DoorState.Locked);
    }

    public void UnlockDoors(Vehicle vehicle)
    {
        if (vehicle.Doors == DoorState.Unlocked)
        {
            throw new Exception("Doors are already unlocked.");
        }
        vehicle.SetDoorState(DoorState.Unlocked);
    }
}