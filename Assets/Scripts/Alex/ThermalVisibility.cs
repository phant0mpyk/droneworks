using UnityEngine;

public class ThermalVisibility : MonoBehaviour
{
    [SerializeField] private Material glow;
    void Start()
    {
        GameObject thermalObject = new GameObject("ThermalObject");
        thermalObject.transform.position = transform.position;
        thermalObject.transform.rotation = transform.rotation;
        thermalObject.transform.localScale = transform.localScale;
        thermalObject.transform.parent = transform;
        thermalObject.AddComponent<MeshRenderer>().material = glow;
        thermalObject.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        thermalObject.layer = LayerMask.NameToLayer("NightVision");
        
        
        gameObject.layer = LayerMask.NameToLayer("NoNightVision");
    }
}
