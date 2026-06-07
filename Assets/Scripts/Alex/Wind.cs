using System;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] private float windSpeed;
    [SerializeField] private float airDensity = 1.225f;
    private float dragCoefficient = 1.2f;
    public Vector3 windForce;
    [SerializeField] public Vector3 windDirection;
    [SerializeField] private bool randomDirection = false;
    private float windAngle;
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
    [SerializeField] private LayerMask droneLayerMask;
    private float elapsedTime;

    [Tooltip("The higher the number the shorter the peak")][SerializeField] private float gustShortness = 50;
    [SerializeField] private float gustSinAmplitude = 3;
    [SerializeField] private float gustSinMultiplier = 0.02f;
    [SerializeField] private float windSinAmpltude = 1;
    [SerializeField] private float windSinMultiplier = 0.5f;
    [SerializeField] private float windVariationSpeed;
    [SerializeField] private float windVariationFrequency;

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
        CalculateWindStrength();
    }

    void CalculateWindStrength()
    {
        gustSpeed = windSpeed /3.6f + (float) Math.Sin(Time.unscaledTime *windSinMultiplier) * windSinAmpltude + (float) Math.Abs(gustSinAmplitude*Math.Pow(Math.Sin(Time.unscaledTime * gustSinMultiplier),gustShortness));
        Vector3 velocity = gustSpeed * windDirection.normalized;
        windForce = (tuning* 0.5f * velocity.magnitude * velocity.magnitude * airDensity * (windCollider.size.x * windCollider.size.y / raycastPoints.Count) * dragCoefficient) * velocity.normalized; //calculate wind force from the relative velocity
    }
    
    private void Start()
    {
        if (randomDirection)
        {
            windDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
        }
        elapsedTime = 0;
        windAngle = 0;
        droneRigidbody = drone.GetComponent<Rigidbody>();
        windCollider = GetComponent<BoxCollider>();
        raycastPoints =  new List<GameObject>();
        transform.forward = windDirection;
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

    void VariateWindDirection()
    {
        
        windAngle = (float)(Math.Sin(Math.PI/2 + elapsedTime/windVariationFrequency) * windVariationSpeed * Time.fixedUnscaledDeltaTime);
        print(windAngle);
        windDirection = Quaternion.AngleAxis(windAngle, Vector3.up) * windDirection; 
        transform.forward = windDirection;
    }

    private void Update()
    {
        if (oldWindPrecision != windPrecision || oldRaycastDistance != raycastDistance || oldWindVelocity !=  windSpeed)
        {
            UpdateWind();
        }
        
        transform.position = drone.transform.position;
        
        VariateWindDirection();
        CalculateWindStrength();
        elapsedTime += Time.unscaledDeltaTime;
    }
    private void FixedUpdate()
    {
        
        if (windZone)
        {
            foreach (GameObject point in raycastPoints)
            {
                Physics.Raycast(point.transform.position, -point.transform.forward, out RaycastHit hit, raycastDistance*1.1f, droneLayerMask);
                if (hit.collider != null)
                { 
                    if (hit.collider.tag == "Player")
                    {
                        print("hello");
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
