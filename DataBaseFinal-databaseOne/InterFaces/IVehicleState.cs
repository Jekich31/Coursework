namespace ProjectFIN.InterFaces;

public interface IVehicleState
{
    void StartEngine(models.Vehicle vehicle);
    void StopEngine(models.Vehicle vehicle);
    void LockDoors(models.Vehicle vehicle);
    void UnlockDoors(models.Vehicle vehicle);
}