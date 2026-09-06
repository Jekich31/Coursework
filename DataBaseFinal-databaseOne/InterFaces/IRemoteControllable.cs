namespace ProjectFIN.InterFaces;

public interface IRemoteControllable
{
    void LockDoors();
    void UnlockDoors();
    void StartEngine();
    void StopEngine();
    void UpdateLocation(double lat, double lng);
}