using UnityEngine;
using UnityEngine.Rendering;

public class ToggleThermalVision : MonoBehaviour
{
    private Volume vol;

    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        vol = GetComponent<Volume>();
        cam.cullingMask = cam.cullingMask & ~(1 << LayerMask.NameToLayer("NightVision"));
        vol.enabled = false;
    }

    void ToggleVision()
    {
        if (vol.enabled)
        {
            cam.cullingMask = cam.cullingMask & ~(1 << LayerMask.NameToLayer("NightVision"));
            vol.enabled = false;
        }
        else
        {
            cam.cullingMask = -1;
            vol.enabled = true;
        }
    }

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            ToggleVision();
        }
    }
}
