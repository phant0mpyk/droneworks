using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Wind : MonoBehaviour
{
    [SerializeField] private float windStrength;
    [SerializeField] private Vector3 windDirection;
    [SerializeField] private float windPrecision;
    [SerializeField] private GameObject drone;
    private float raycastDistance = 5;
    float oldWindPrecision;
    private List<GameObject> raycastPoints;
    BoxCollider windCollider;
    bool windZone =  true;
    private Rigidbody droneRigidbody;
    
    void CalculateRaycastPoints()
    {
        foreach (GameObject go in raycastPoints)
        {
            Destroy(go);
        }
        raycastPoints.Clear();
        
        float distanceBetweenPoints = windCollider.size.x / windPrecision;
        int numberOfPointsY = (int) (windCollider.size.y / distanceBetweenPoints); // I want the distance between them to be the same length- and width-wise
        for (int i = 0; i < windPrecision; ++i)
        {
            for (int o = 0; o < numberOfPointsY; ++o)
            {
                GameObject newPoint = new GameObject($"Point {i} {o}");
                newPoint.transform.localPosition = new Vector3(i * distanceBetweenPoints - windCollider.size.x * 0.5F, o * distanceBetweenPoints - windCollider.size.y * 0.5F, raycastDistance);
                newPoint.transform.parent = this.gameObject.transform;
                raycastPoints.Add(newPoint);
            }
        }
    }
    
    private void Start()
    {
        droneRigidbody = drone.GetComponent<Rigidbody>();
        windCollider = GetComponent<BoxCollider>();
        raycastPoints =  new List<GameObject>();
        
        CalculateRaycastPoints();
        oldWindPrecision = windPrecision;
        
        transform.forward = windDirection;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (GameObject point in raycastPoints)
            {
                Gizmos.DrawWireSphere(point.transform.position, 0.05f);
                Gizmos.DrawRay(point.transform.position, -point.transform.forward);
            }    
        }
    }
    

    private void FixedUpdate()
    {
        if (oldWindPrecision != windPrecision)
        {
            CalculateRaycastPoints();
            oldWindPrecision = windPrecision;
        }

        if (windDirection != transform.forward)
        {
            transform.forward = windDirection;
        }
        transform.position = drone.transform.position;
        
        
        if (windZone)
        {
            foreach (GameObject point in raycastPoints)
            {
                Physics.Raycast(point.transform.position, -point.transform.forward, out RaycastHit hit, raycastDistance*1.5f);
                if (hit.collider != null)
                {
                    print($"{point.name} hit {hit.collider.gameObject.name}");
                    if (hit.collider.tag == "Drone")
                    {
                        droneRigidbody.AddForceAtPosition((Time.deltaTime * windStrength / raycastPoints.Count) * -point.transform.forward, point.transform.position, ForceMode.Force);
                    }
                }
            }        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "NoWindZone")
        {
            windZone = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "NoWindZone")
        {
            windZone = true;
        }
    }
}
