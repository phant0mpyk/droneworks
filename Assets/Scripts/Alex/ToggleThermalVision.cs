using UnityEngine;
using UnityEngine.Rendering;

public class ToggleThermalVision : MonoBehaviour
{
    private Volume vol;
    private Camera cam;
    private float defaultLighting;
    [SerializeField] private float nightVisionLighting;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        vol = GetComponent<Volume>();
        cam.cullingMask = ~(1 << LayerMask.NameToLayer("NightVision"));
        vol.enabled = false;
        defaultLighting = RenderSettings.ambientIntensity;
    }

    public void ToggleVision()
    {
        if (vol.enabled)
        {
            cam.cullingMask = ~(1 << LayerMask.NameToLayer("NightVision"));
            vol.enabled = false;
            RenderSettings.ambientIntensity = defaultLighting;
        }
        else
        {
            cam.cullingMask = ~(1 << LayerMask.NameToLayer("NoNightVision"));
            vol.enabled = true;
            RenderSettings.ambientIntensity = nightVisionLighting;
        }
    }

    

}
