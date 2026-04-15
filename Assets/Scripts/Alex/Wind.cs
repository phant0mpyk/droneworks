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
    [SerializeField] private float raycastDistance = 5;
    private GameObject ping;
    float oldWindPrecision;
    float oldRaycastDistance;
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
                newPoint.transform.parent = this.gameObject.transform;
                newPoint.transform.localPosition = new Vector3(i * distanceBetweenPoints - windCollider.size.x * 0.5F, o * distanceBetweenPoints - windCollider.size.y * 0.5F, raycastDistance);
                newPoint.transform.forward = transform.forward;
                raycastPoints.Add(newPoint);
            }
        }
    }

    void UpdateWind()
    {
        CalculateRaycastPoints();
        oldWindPrecision = windPrecision;
        oldRaycastDistance = raycastDistance;
        transform.forward = windDirection;
        ping.transform.localPosition = new Vector3(0, 0, raycastDistance);
    }
    
    private void Start()
    {
        droneRigidbody = drone.GetComponent<Rigidbody>();
        windCollider = GetComponent<BoxCollider>();
        raycastPoints =  new List<GameObject>();
        ping = transform.GetChild(0).gameObject;
        
        UpdateWind();
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
        if (oldWindPrecision != windPrecision || oldRaycastDistance != raycastDistance)
        {
            UpdateWind();
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
                Physics.Raycast(point.transform.position, -point.transform.forward, out RaycastHit hit, raycastDistance*1.1f);
                if (hit.collider != null)
                { 
                    if (hit.collider.tag == "Drone")
                    {
                        Physics.Raycast(hit.point, point.transform.forward, out RaycastHit hitBack, raycastDistance*1.1f);
                        if (hitBack.collider == null)
                        {
                            continue;
                        }
                        if (hitBack.collider.tag == "Wind")
                        {
                            droneRigidbody.AddForceAtPosition((Time.deltaTime * windStrength / raycastPoints.Count) * -point.transform.forward, hit.point, ForceMode.Force);
                        }
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
