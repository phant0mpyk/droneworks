using UnityEngine;

public class PropellerSound : MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField] private AudioSource soundSource;
    private FlightController controller;
    private DroneInputManager inputManager;
    [SerializeField] private float pitch;
    void Start()
    {
        controller = GetComponent<FlightController>();
        inputManager = GetComponent<DroneInputManager>();
        soundSource.clip = sound;
        soundSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputManager.IsArmed && !soundSource.isPlaying)
        {
            soundSource.Play();
        } else if (!inputManager.IsArmed)
        {
            soundSource.Stop();
        }

        soundSource.pitch = pitch + inputManager.GetThrottleAxis()/2;
    }
}
