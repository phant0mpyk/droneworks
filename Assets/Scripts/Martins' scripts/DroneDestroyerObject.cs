using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class DroneDestroyerObject : MonoBehaviour
{
    private BoxCollider boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.name == "DroneDJIMini2")
        {
            FlightController flightController = other.gameObject.GetComponentInParent<FlightController>();
            if (flightController != null)
            {
                flightController.DestroyDrone();
            }
        }
    }
}
