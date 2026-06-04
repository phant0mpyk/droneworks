using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] private float windSpeed;
    [SerializeField] private float airDensity = 1.225f;
    private float dragCoefficient = 1.2f;
    public Vector3 windForce;
    [SerializeField] public Vector3 windDirection;
    [SerializeField] private float windPrecision;
    [SerializeField] private GameObject drone;
    [SerializeField] private float raycastDistance = 5;
    float oldWindPrecision;
    float oldRaycastDistance;
    float oldWindVelocity;
    private List<GameObject> raycastPoints;
    BoxCollider windCollider;
    bool windZone =  true;
    private Rigidbody droneRigidbody;
    [SerializeField] private float tuning;

    [SerializeField] private float gustAmplitude;
    [SerializeField] private float gustSinMultiplier = 2;
    public float gustSpeed;
    
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
                newPoint.transform.parent = gameObject.transform;
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
        oldWindVelocity = windSpeed;
        transform.forward = windDirection;
        CalculateWindStrength();
    }

    void CalculateWindStrength()
    {
        gustSpeed = windSpeed /3.6f + (float) Math.Sin(Time.unscaledTime *gustSinMultiplier) * gustAmplitude + (float) Math.Abs(5*Math.Pow(Math.Sin(Time.unscaledTime/50),20));
        Vector3 velocity = gustSpeed * windDirection.normalized;
        windForce = (tuning* 0.5f * velocity.magnitude * velocity.magnitude * airDensity * (windCollider.size.x * windCollider.size.y / raycastPoints.Count) * dragCoefficient) * velocity.normalized; //calculate wind force from the relative velocity
    }
    
    private void Start()
    {
        droneRigidbody = drone.GetComponent<Rigidbody>();
        windCollider = GetComponent<BoxCollider>();
        raycastPoints =  new List<GameObject>();
        
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
        if (oldWindPrecision != windPrecision || oldRaycastDistance != raycastDistance || oldWindVelocity !=  windSpeed)
        {
            UpdateWind();
        }

        if (windDirection != transform.forward)
        {
            transform.forward = windDirection;
        }
        transform.position = drone.transform.position;
        
        
        CalculateWindStrength();
        if (windZone)
        {
            foreach (GameObject point in raycastPoints)
            {
                Physics.Raycast(point.transform.position, -point.transform.forward, out RaycastHit hit, raycastDistance*1.1f);
                if (hit.collider != null)
                { 
                    if (hit.collider.tag == "Player")
                    {
                        float amplitude = Mathf.Max(0, Vector3.Dot(point.transform.forward.normalized, hit.normal.normalized));
                        amplitude *= amplitude;
                        droneRigidbody.AddForceAtPosition(amplitude * -windForce, hit.point, ForceMode.Force);
                        
                    }
                }
            }        
        }
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
