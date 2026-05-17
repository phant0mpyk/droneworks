using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DroneDestroyerObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FlightController flightController))
        {
            if (!flightController.GetDroneDestroyed())
            {
                flightController.DestroyDrone();
            }
        }
    }
}