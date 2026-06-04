using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            DroneBoundaryEnforcer script =  other.GetComponent<DroneBoundaryEnforcer>();
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
            script.startPosition = hit.point + Vector3.up * other.GetComponent<BoxCollider>().size.y;
            script.startRotation = transform.rotation;
        }
    }
}
