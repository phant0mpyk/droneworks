using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ActivateNightVision : MonoBehaviour
{
    [SerializeField]
    SFXManager sfxManager;
    [SerializeField]
    AudioSFXScriptableObject nightvisionSFX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            col.TryGetComponent<FlightController>(out FlightController flightController);{
                flightController.ToggleThermalVision();
                sfxManager?.PlayVoicelineSFX(nightvisionSFX, nightvisionSFX.audioClipEntry.clipName, Camera.main.GetComponent<AudioSource>());
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            col.TryGetComponent<FlightController>(out FlightController flightController);{
                flightController.ToggleThermalVision();
            }
        }
    }
}
