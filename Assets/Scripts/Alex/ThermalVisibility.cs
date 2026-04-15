using UnityEngine;

public class ThermalVisibility : MonoBehaviour
{
    [SerializeField] private Material glow;
    void Start()
    {
        GameObject thermalObject = new GameObject("ThermalObject");
        thermalObject.transform.position = transform.position;
        thermalObject.transform.rotation = transform.rotation;
        thermalObject.transform.parent = transform;
        thermalObject.transform.localScale = Vector3.one * 1.05f;
        thermalObject.AddComponent<MeshRenderer>().material = glow;
        thermalObject.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        thermalObject.layer = 8; //Night Vision Layer
    }
}
