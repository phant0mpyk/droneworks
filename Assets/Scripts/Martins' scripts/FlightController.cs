using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


[RequireComponent(typeof(DroneInputManager))]
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FlightController : MonoBehaviour
{
    DroneInputManager inputManager;

    [SerializeField]
    DroneBatteryManager battery;

    Rigidbody droneRigidbody;

    bool droneActive = false;

    [Tooltip("Array of the drone propellers")]
    [SerializeField] 
    GameObject[] propellers;
    DronePropellerScript[] propellerScripts;

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;
    float currAltitude = 0f;

    public enum FlightMode { Stabilized, Acrobatic }

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
    float maxRPM;

    float originalMaxRPM;

    [Tooltip("RPM at which the drone hovers when throttle stick is at 50%. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float hoverRPMPercentage;

    float minRPM;
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

    void Awake()
    {
        droneRigidbody = GetComponent<Rigidbody>();
        inputManager = GetComponentInChildren<DroneInputManager>();
        battery = GetComponentInChildren<DroneBatteryManager>();
        propellerScripts = new DronePropellerScript[propellers.Length];
        for (int i = 0; i < propellers.Length; i++)        {
            propellerScripts[i] = propellers[i].GetComponent<DronePropellerScript>();
        }
        originalMaxRPM = maxRPM;
    }
    void Start()
    {
        droneActive = true;
    }

    //tip for values (English to Slovak)
    //current = prud (Ampers), voltage = napatie (Volts), power = vykon (Watts), energy = energia (Wh), capacity = kapacita (mAh), resistance = odpor (Ohms)
    void Update()
    {
        
        if (droneActive)
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
        // Debug.Log(GetBatteryPercentageWithBatterySafety() + "% battery remaining. Current battery voltage: " + currBatteryVoltage + "V");
        //the drone will first calculate the voltage drop that affects the maxRPM after which it will apply the change to the maxRPM possible
        if(droneActive && inputManager)
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
            case FlightMode.Stabilized:
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
}
