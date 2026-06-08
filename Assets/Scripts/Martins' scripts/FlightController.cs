using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;


[RequireComponent(typeof(DroneInputManager))]
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FlightController : MonoBehaviour
{
    DroneInputManager inputManager;
    DroneBoundaryEnforcer boundaryEnforcer;
    DroneBatteryManager battery;    

    [SerializeField]
    DroneCameraManager cameraManager;

    [SerializeField]
    ToggleThermalVision toggleThermalVision;

    [SerializeField]
    SpawnMarker ping;

    Rigidbody droneRigidbody;

    BoxCollider droneCollider;

    bool droneActive = false;

    bool droneArmed = false;

    [Tooltip("Array of the drone propellers")]
    [SerializeField] 
    GameObject[] propellers;
    DronePropellerScript[] propellerScripts;

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;

    public enum FlightMode { StabilizedHeight,StabilizedThrottle, Acrobatic }

    [SerializeField]
    [Header("Flight Mode")]
    private FlightMode flightMode;

    [Header("Motor RPM Settings")]
    [Tooltip("Change in RPM. Per 1 propeller.")]
    [SerializeField]
    float deltaRPM;
    
    [Tooltip("RPM at which the drone is idle, so when throttle is all the way down. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float minRPMPercentage;

    [Tooltip("Maximum RPM the drone propeller can reach. Gets calculated by itself and is influenced by the battery. Per 1 propeller.")]
    [SerializeField]
    public float maxRPM;

    float originalMaxRPM;

    [Tooltip("RPM at which the drone hovers when throttle stick is at 50%. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float hoverRPMPercentage;

    public float minRPM;
    float hoverRPM;

    [Header("Acrobatic Flight Mode Settings")]
    [Tooltip("Rotation speed multiplier for tilting (roll/pitch) the drone for fine-tuning.")]
    [SerializeField]
    float tiltAcrobaticRotationMultiplier = 1f;

    [Tooltip("Rotation speed multiplier for yawing the drone for fine-tuning.")]
    [SerializeField]
    float yawAcrobaticRotationMultiplier = 1f;

    [Header("Stabilized Flight Mode Settings")]
    //stabilization settings 
    [Tooltip("Maximum pitch/roll angle for the propeller.")]
    [SerializeField]
    Vector3 maxTiltAngle;

    [Tooltip("Rotation speed multiplier for tilting (roll/pitch) the drone for fine-tuning.")]
    [SerializeField]
    float tiltStabilizedRotationMultiplier = 1f;

    [Tooltip("Rotation speed multiplier for yawing the drone for fine-tuning.")]
    [SerializeField]
    float yawStabilizedRotationMultiplier = 1f;

    [Tooltip("Stabilized height mode maximum alg it can hover at.")]
    [SerializeField]
    float maxAGL;
    float currAGL = 0f;
    float desiredAGL;
    [Tooltip("Adjusting the strenght of AGL correction.")]
    [SerializeField]
    float aglCorrectionStrength;
    
    Vector3 worldUpVector;
    Vector3 droneUpVector;
    Vector3 droneForwardVector;

    void Awake()
    {
        droneRigidbody = GetComponent<Rigidbody>();
        droneCollider = GetComponent<BoxCollider>();
        inputManager = GetComponentInChildren<DroneInputManager>();
        battery = GetComponentInChildren<DroneBatteryManager>();
        boundaryEnforcer = GetComponent<DroneBoundaryEnforcer>();
        propellerScripts = new DronePropellerScript[propellers.Length];
        for (int i = 0; i < propellers.Length; i++)        {
            propellerScripts[i] = propellers[i].GetComponent<DronePropellerScript>();
        }
        originalMaxRPM = maxRPM;
    }
    void Start()
    {
        droneActive = true;
        worldUpVector = Vector3.up.normalized;
        droneUpVector = transform.up.normalized;
        droneForwardVector = transform.forward.normalized;
    }

    //tip for values (English to Slovak)
    //current = prud (Ampers), voltage = napatie (Volts), power = vykon (Watts), energy = energia (Wh), capacity = kapacita (mAh), resistance = odpor (Ohms)
    void Update()
    {
        FlightMode currentFlightMode = flightMode;
        if(currentFlightMode == FlightMode.StabilizedHeight || currentFlightMode == FlightMode.StabilizedThrottle)
        {
            droneUpVector = transform.up.normalized;
            cameraManager.AdjustGimbalAngle(worldUpVector, droneForwardVector);
        }
        if (droneActive && droneArmed)
        {
            //update each frame cuz it needs to respond to the battery voltage dropping the maxRPM over time as battery runs out
            minRPM = minRPMPercentage/100 * maxRPM;
            hoverRPM = hoverRPMPercentage/100 * maxRPM;
            //changing of current input layout
            // Debug.Log(yawAxis +" " + pitchAxis +  " " + rollAxis + " " + throttleAxis);
            // Debug.Log("Controller active: " + controllerInputActive + " Keyboard active: " + keyboardInputActive);
        }
    }

    //Applies thrust and rotations based on the current flight mode. 
    //Acrobatic flight mode calculates thrust realistically based on RPM on all 4 propellers and applies it with physics
    //Stabilized flight mode applies same as acrobatic but rotates it using transform instead of torque (also disregards that during currRPM calculations)
    void FixedUpdate()
    {
        if (!droneActive || !inputManager) return;
        // Debug.Log(GetBatteryPercentageWithBatterySafety() + "% battery remaining. Current battery voltage: " + currBatteryVoltage + "V");
        //the drone will first calculate the voltage drop that affects the maxRPM after which it will apply the change to the maxRPM possible
        if(droneActive && droneArmed && inputManager)
        {
            float voltageDrop = battery.CalculateVoltageDrop(inputManager.GetThrottleAxis());
            maxRPM = battery.CalculateMaxRPMAfterVoltageDrop(voltageDrop);
            CalculateAndApplyCurrRPM(inputManager.GetThrottleAxis(), inputManager.GetYawAxis(), inputManager.GetPitchAxis(), inputManager.GetRollAxis());
        }
        // Debug.Log("Current max RPM: " + maxRPM + " Current hover RPM: " + hoverRPM + " Current min RPM: " + minRPM);
    }

    void CalculateAndApplyCurrRPM(float _throttleAxis, float yawAxis, float _pitchAxis, float _rollAxis)
    {
        float pitchDelta = 0f;
        float rollDelta = 0f;
        float yawDelta = 0f;
        pitchDelta = _pitchAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        rollDelta = _rollAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        yawDelta = yawAxis * deltaRPM * yawAcrobaticRotationMultiplier;
        desiredAGL = maxAGL/2 + (maxAGL/2 * _throttleAxis);
        switch (flightMode)
        {
            case FlightMode.Acrobatic:
                for (int i = 0; i < propellerScripts.Length; i++)
                {
                    float currRPM = hoverRPM + _throttleAxis * (maxRPM - hoverRPM);
                    //Adjust currRPM based on propeller position
                    switch (propellerScripts[i].GetPropellerPosition())
                    {
                        case DronePropellerScript.PropellerPosition.FrontLeft:
                            currRPM = currRPM - pitchDelta + rollDelta;                 
                            break;
                        case DronePropellerScript.PropellerPosition.FrontRight:
                            currRPM = currRPM - pitchDelta - rollDelta;
                            break;
                        case DronePropellerScript.PropellerPosition.BackLeft:   
                            currRPM = currRPM + pitchDelta + rollDelta;
                            break;
                        case DronePropellerScript.PropellerPosition.BackRight:
                            currRPM = currRPM + pitchDelta - rollDelta;
                            break;
                        default:
                            break;
                    }     
                    // Adjust currRPM depending on the propeller rotation direction as well
                    int yawSign = (int)propellerScripts[i].GetPropellerRotation();
                    currRPM += yawDelta * yawSign;
                    currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
                    propellerScripts[i].ApplyPropellerForce(currRPM, airDensity, flightMode);
                }
                break;
                
            case FlightMode.StabilizedThrottle:
                for(int i = 0; i < propellerScripts.Length; i++)
                {
                    float currRPM = hoverRPM + _throttleAxis * (maxRPM - hoverRPM);
                    currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
                    propellerScripts[i].ApplyPropellerForce(currRPM, airDensity, flightMode);
                }
                //Simplified version of drone rotation for stabilized mode without 4 propellers changing torque, but just static limited rotation
                Quaternion targetRotation = Quaternion.Euler(_pitchAxis * maxTiltAngle.x, transform.eulerAngles.y, -_rollAxis * maxTiltAngle.z);
                // Debug.Log("Target rotation: " + targetRotation + " Pitch: " + pitchAxis + "Roll: " + rollAxis);
                //Slerp is better than lerp for this case, because it simulates the rotation in a more natural curvey instead of lerp which is more linear 
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltStabilizedRotationMultiplier * Time.fixedDeltaTime);
                //rotation for yaw is separate because it should not be affected by the tilt of the drone for stabilized mode, so it is applied on the world y axis
                transform.Rotate(Vector3.up, yawAxis * yawStabilizedRotationMultiplier * Time.fixedDeltaTime, Space.World);
                break;
            //this version takes into account the current above ground level and adjusts the throttle accordingly so it hovers around that height
            //aglError is there to add to the currRPM 
            case FlightMode.StabilizedHeight:
                for(int i = 0; i < propellerScripts.Length; i++)
                {
                    float baseRPM = hoverRPM + _throttleAxis * (maxRPM - hoverRPM);
                    float aglError = desiredAGL - currAGL;
                    float aglErrorNormalized = (aglError / maxAGL) * aglCorrectionStrength;
                    float currRPM = baseRPM + aglErrorNormalized * (maxRPM - hoverRPM);
                    currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
                    propellerScripts[i].ApplyPropellerForce(currRPM, airDensity, flightMode);
                }
                //Simplified version of drone rotation for stabilized mode without 4 propellers changing torque, but just static limited rotation
                Quaternion _targetRotation = Quaternion.Euler(_pitchAxis * maxTiltAngle.x, transform.eulerAngles.y, -_rollAxis * maxTiltAngle.z);
                // Debug.Log("Target rotation: " + targetRotation + " Pitch: " + pitchAxis + "Roll: " + rollAxis);
                //Slerp is better than lerp for this case, because it simulates the rotation in a more natural curvey instead of lerp which is more linear 
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, tiltStabilizedRotationMultiplier * Time.fixedDeltaTime);
                //rotation for yaw is separate because it should not be affected by the tilt of the drone for stabilized mode, so it is applied on the world y axis
                transform.Rotate(Vector3.up, yawAxis * yawStabilizedRotationMultiplier * Time.fixedDeltaTime, Space.World);
                break;
            default:
                Debug.LogWarning("Unknown flight mode: " + flightMode);
                break;
        }
    }

    public FlightMode GetCurrFlightMode()
    {
        return flightMode;
    }

    public int GetPropellerCount()
    {
        return propellerScripts.Length;
    }

    public void OnCameraToggle()
    {
        cameraManager.ToggleCameraPosition();
    }

    public void RotateGimbalUp()
    {
        cameraManager.RotateCameraGimbal(DroneCameraManager.CameraGimbalRotationDirection.Up);
    }
    public void RotateGimbalDown()
    {
        cameraManager.RotateCameraGimbal(DroneCameraManager.CameraGimbalRotationDirection.Down);
    }

    public void RotateGimbalNone()
    {
        cameraManager.RotateCameraGimbal(DroneCameraManager.CameraGimbalRotationDirection.None);
    }

    public void ToggleThermalVision()
    {
        toggleThermalVision.ToggleVision();
    }

    public void Ping()
    {
        ping.TryToPing();
    }

    //added more options if you need to set it from the tutorial or wherever
    //added by V
    public void SetFlightModeAcrobatic()
    {
        flightMode = FlightMode.Acrobatic;
        Debug.Log("Flight Mode Switched to: ACROBATIC");
    }

    public void SetFlightModeStabilizedThrottle()
    {
        flightMode = FlightMode.StabilizedThrottle;
        Debug.Log("Flight Mode Switched to: STABILIZEDTHROTTLE");
    }

    public void SetFlightModeStabilizedHeight()
    {
        flightMode = FlightMode.StabilizedHeight;
        Debug.Log("Flight Mode Switched to: STABILIZEDHEIGHT");
    }
    // //end of added by V

    //disables drone and applied physics as well as maxRPM, starts respawning and the UI glitch effect
    //the disabled physics are also applied in V's script, but because I use it later than he does and that it collides with the object, It needs to be applied sooner to avoid problems with 
    public void DestroyDrone()
    {
        if (!droneActive) return;
        droneActive = false;
        droneArmed = false;
        droneRigidbody.isKinematic = true; 
        Debug.Log("Drone destroyed");
        StartCoroutine(RespawnCoroutine(2f));
    }

    //disables colliders of the drone until the drone respawns so that it doesn't get stuck in the destroying object when it hits hit
    //also waits for realtime seconds because Alex's glitch effect script sets Time.timeScale to 0
    private IEnumerator RespawnCoroutine(float respawnTime)
    {
        if (droneCollider) droneCollider.enabled = false;
        yield return new WaitForSecondsRealtime(respawnTime);
        Respawn();
        yield return new WaitForSecondsRealtime(0.1f); 
        if (droneCollider) droneCollider.enabled = true;
    }

    //applies physics back and uses V's teleport script to teleport the drone back to the start position
    void Respawn()
    {
        //needs to be added in even if V's already doing it for some reason
        //I guess it kindof skips V's rigidbody changes when it teleports the drone back to the start position
        droneRigidbody.isKinematic = false;
        droneRigidbody.linearVelocity = Vector3.zero;
        droneRigidbody.angularVelocity = Vector3.zero;
        maxRPM = 0f; 
        boundaryEnforcer.TeleportBackToStart();
        Debug.Log("Drone respawned");
        droneActive = true;
    }
    public bool GetDroneDestroyed()
    {
        return !droneActive;
    }

    public void SetDroneArmed(bool armed)
    {
        droneArmed = armed;
    }

    public void SetCurrentAGL(float agl)
    {
        currAGL = agl;
    }

}
