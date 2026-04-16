using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

public class SpawnMarker : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private float markerLifetime;
    [SerializeField] private GameObject floatingArrow;
    LayerMask mask;
    private bool ready = true;
    void Start()
    {
        mainCam = Camera.main;
        mask = ~(1 << LayerMask.NameToLayer("Drone") |  1 << LayerMask.NameToLayer("Ignore Raycast"));
    }

    public void TryToPing()
    {
        Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out RaycastHit hit, math.INFINITY, mask);
        if (hit.point != Vector3.zero)
        {
            StartCoroutine(SpawnArrow(hit.point, Quaternion.LookRotation(Vector3.Cross((hit.normal + Vector3.one).normalized,hit.normal),hit.normal)));
        }
    }

    private IEnumerator SpawnArrow(Vector3 position, Quaternion rotation)
    {
        ready = false;
        GameObject newArrow = Instantiate(floatingArrow);
        newArrow.transform.position = position;
        newArrow.transform.rotation = rotation;
        
        yield return new WaitForSeconds(markerLifetime);
        Destroy(newArrow);
        ready = true;
        
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.M) && ready)
        // {
        //     Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out RaycastHit hit, math.INFINITY, mask);
        //     if (hit.point != Vector3.zero)
        //     {
        //         StartCoroutine(SpawnArrow(hit.point, Quaternion.LookRotation(Vector3.Cross((hit.normal + Vector3.one).normalized,hit.normal),hit.normal)));
        //     }
        // } 
    }
}
