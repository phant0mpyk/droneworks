using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionSensor : MonoBehaviour
{
    private float colliderCount;
    private float distance = 0;
    [SerializeField] private RawImage sensorImage;
    [SerializeField] private float minDistance;
    private float maxDistance;
    [SerializeField] private GameObject drone;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxDistance = GetComponent<CapsuleCollider>().height;
    }

    // Update is called once per frame
    void Update()
    {
        if (colliderCount == 0)
        {
            sensorImage.color = new Color(0,0,0,0);
        }
        else
        {
            sensorImage.color = new Color(1, Mathf.Clamp((distance-minDistance)/(maxDistance-minDistance),0,1), 0, 1-Mathf.Clamp((distance-minDistance)/(maxDistance-minDistance),0,1));
        }
        distance = 100;
    }

    private void OnTriggerEnter(Collider other)
    {
        ++colliderCount;
    }

    private void OnTriggerExit(Collider other)
    {
        --colliderCount;
    }

    private void OnTriggerStay(Collider other)
    {
        float newDistance = Vector3.Distance(other.ClosestPoint(drone.transform.position),  drone.transform.position);
        if(newDistance<distance)
        {
            distance = newDistance;
            print(distance);
        }
        
    }
}
