using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DroneScript : MonoBehaviour
{
    Rigidbody droneRigidbody;
    float throttleAxis;
    float pitchAxis;
    float yawAxis;
    float rollAxis;

    bool droneActive = true;

    [Header("Controls")]
    [SerializeField] 
    InputActionReference flyWASD;

    [SerializeField] 
    InputActionReference flyArrows;

    bool keyboardInputActive = false;

    [SerializeField]
    InputActionReference leftStickInputAxis;

    [SerializeField]
    InputActionReference rightStickInputAxis;

    bool controllerInputActive = false;

    [SerializeField]
    Camera droneCamera;

    [Tooltip("Camera up-tilt in degrees. 0-10 is standard for cinematic drones, 10-25 for FPV freestyle drones, above 25 for racing drones.")]
    [SerializeField]
    float cameraTilt = 0f;

    [Tooltip("Array of the drone propellers")]
    [SerializeField] 
    GameObject[] propellers;
    PropellerScript[] propellerScripts;

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;
    float currAltitude = 0f;

    //the battery settings need to be set by the drone model individually, however flight time can be adjusted as needed
    [Header("Battery Settings")]
    [Tooltip("Battery capacity in mAh.")]
    [SerializeField]
    float batteryCapacity;

    [Tooltip("Internal resistance of the battery in Ohms. Won't include cable resistance, which can be significant for high current draw, but for simplicity it is not included in the current version of the script.")]
    [SerializeField]
    float internalBatteryResistance;

    [Tooltip("This is the RPM per volt of applied voltage. Used to calculate maxRPM with efficiency and battery voltage. This however doesn't take mass into account (measured on just the motor of that propeller), which is why it uses propellerLoadedEfficiency as well to calculate maxRPM representing the drone capability with load")]
    [SerializeField]
    float KV;

    [Tooltip("This is the efficiency of the motor and propeller combination. Used to calculate maxRPM, because the drone with load (such as the drone itself with camera) doesn't reach 100% efficiency. In percentage.")]
    [SerializeField]
    float propellerLoadedEfficiency;

    [Tooltip ("Number of cells in the battery. In Series(S).")]
    [SerializeField]
    int batteryCells;

    [Tooltip("Maximum voltage the battery has. Per 1 cell. Standard maximum is 4.2V for LiPo/LiIon batteries, although nominal voltage is usually 3.7V.")]
    [SerializeField]
    float maxCellVoltage = 4.2f;

    [Tooltip("Nominal voltage the battery has. Per 1 cell. Standard nominal voltage is 3.7V for LiPo/LiIon batteries.")]
    [SerializeField]
    float nominalCellVoltage = 3.7f;

    [Tooltip("Minimum voltage in Volts. The battery will shutdown at this value. Same for LiPo / LiIon batteries and standard is 3.0V. Per 1 cell.")]
    [SerializeField]
    float minCellVoltage = 3.0f;

    [Tooltip("Battery discharge that you allow during the flight. As batteries can be damaged if fully discharged, it's common practice never to discharge them by more than 80%. In percentage.")]
    [SerializeField]
    float batteryMaxDischargePercentage = 80f;

    [Tooltip("Average current draw in Amps. Used to calculate battery draining. Per 1 propeller.")]
    [SerializeField]
    float averageBatteryCurrentDraw = 0f;

    [Tooltip("For a battery warning at a certain Voltage. Per 1 cell.")]
    [SerializeField]
    float batteryWarningCellVoltage = 3.5f;

    [Tooltip("Maximum possible flight time of the drone in minutes.")]
    [SerializeField]
    float maxFlightTimeMinutes;

    float remainingFlightTimeMinutes;

    [SerializeField]
    float energy;

    [SerializeField]
    float currBatteryChargeWithSafetyLimit;

    [SerializeField]
    float currBatteryChargeOverall;

    public float currBatteryVoltage { get; private set; }

    public enum FlightMode { Stabilized, Acrobatic }

    [SerializeField]
    [Header("Flight Mode")]
    private FlightMode stabilizationFlightMode;

    [Header("Motor RPM Settings")]
    [Tooltip("Change in RPM. Per 1 propeller.")]
    [SerializeField]
    float deltaRPM;
    
    [Tooltip("RPM at which the drone is idle, so when throttle is all the way down. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float minRPMPercentage;

    [Tooltip("Maximum RPM the drone propeller can reach. Per 1 propeller.")]
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
    }
    void Start()
    {
        propellerScripts = new PropellerScript[propellers.Length];
        for (int i = 0; i < propellers.Length; i++)        {
            propellerScripts[i] = propellers[i].GetComponent<PropellerScript>();
        }

        flyWASD.action.Enable();
        flyArrows.action.Enable();
        leftStickInputAxis.action.Enable();
        rightStickInputAxis.action.Enable();
        
        //calculates max battery charge based off of the dischargePercentage
        //irl this is set to conserve the battery, so it doesn't let you discharge it fully
        float usableBatteryCapacity = batteryCapacity * (batteryMaxDischargePercentage / 100f);
        currBatteryChargeWithSafetyLimit = usableBatteryCapacity;
        currBatteryChargeOverall = batteryCapacity;
        remainingFlightTimeMinutes = maxFlightTimeMinutes;

        //the power shouldn't change based on the remaining flight time, otherwise the voltage drop would be much greater than expected, because the average didn't change
        // remainingFlightTimeMinutes -= Time.deltaTime / 60f; // Convert seconds to minutes
        // float power = energy/(remainingFlightTimeMinutes/60f); 

        //calculates the average current draw per propeller with power using predetermined energy, maxFlightTime and nominal cell voltage
        //before it wasn't calculated with power but from the battery capacity and flight time, which lead to the voltage drop not being realistic and dropping too fast
        //calculates the electrical power in Watts, that is produced by the drone with it's motors based on the maximum flight time it has. It could also be calculated using a predetermined averageBatteryCurrentDraw.
        float power = energy/(maxFlightTimeMinutes/60f);
        //average current draw formula per propeller with power and nominal cell voltage
        //could also be predefined so we wouldn't need to calculate it based on the max flight time, but this way we can adjust the flight time how we want it. Don't set it too low though.
        averageBatteryCurrentDraw = power/(nominalCellVoltage * propellerScripts.Length);
        Debug.Log("Average current draw per propeller: " + averageBatteryCurrentDraw + "A");
        originalMaxRPM = maxRPM;
    }

    //tip for values (English to Slovak)
    //current = prud (Ampers), voltage = napatie (Volts), power = vykon (Watts), energy = energia (Wh), capacity = kapacita (mAh), resistance = odpor (Ohms)
    void Update()
    {
        if (droneActive)
        {
            CheckCellVoltage();
            //update each frame cuz it needs to respond to the battery voltage dropping the maxRPM over time as battery runs out
            minRPM = minRPMPercentage/100 * maxRPM;
            hoverRPM = hoverRPMPercentage/100 * maxRPM;
            //tilt of the camera
            droneCamera.transform.localRotation = Quaternion.Euler(-cameraTilt, 0f, 0f);
            //changing of current input layout
            if(flyWASD.action.triggered || flyArrows.action.triggered)
            {
                controllerInputActive = false;
                keyboardInputActive = true;
            }else if (leftStickInputAxis.action.triggered || rightStickInputAxis.action.triggered)
            {
                controllerInputActive = true;
                keyboardInputActive = false;
            }
            //input depending on the currently active controls
            //looks like action.triggered only fires once when the input is first detected so there are booleans which indicate which layout is active instead
            if (controllerInputActive)
            {
                Vector2 flyLeftStickInput = leftStickInputAxis.action.ReadValue<Vector2>();
                Vector2 flyRightStickInput = rightStickInputAxis.action.ReadValue<Vector2>();
                throttleAxis = flyLeftStickInput.y;
                yawAxis = flyLeftStickInput.x;
                pitchAxis = flyRightStickInput.y;
                rollAxis = flyRightStickInput.x;
            }else if (keyboardInputActive)
            {
                Vector2 flyWASDInput = flyWASD.action.ReadValue<Vector2>();
                throttleAxis = flyWASDInput.y; 
                yawAxis = flyWASDInput.x;     
                Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
                rollAxis = arrowInput.x;
                pitchAxis = arrowInput.y;
            }
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
        if(droneActive)
        {
            CalculateVoltageDrop();
            CalculateAndApplyCurrRPM();
        }
        // Debug.Log("Current max RPM: " + maxRPM + " Current hover RPM: " + hoverRPM + " Current min RPM: " + minRPM);
    }

    void CalculateAndApplyCurrRPM()
    {
        float pitchDelta = 0f;
        float rollDelta = 0f;
        float yawDelta = 0f;
        pitchDelta = pitchAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        rollDelta = rollAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        yawDelta = yawAxis * deltaRPM * yawAcrobaticRotationMultiplier;
        switch (stabilizationFlightMode)
        {
            case FlightMode.Acrobatic:
                for (int i = 0; i < propellerScripts.Length; i++)
                {
                    float currRPM = hoverRPM + throttleAxis * (maxRPM - hoverRPM);
                    //Adjust currRPM based on propeller position
                    switch (propellerScripts[i].GetPropellerPosition())
                    {
                        case PropellerScript.PropellerPosition.FrontLeft:
                            currRPM = currRPM - pitchDelta + rollDelta;                 
                            break;
                        case PropellerScript.PropellerPosition.FrontRight:
                            currRPM = currRPM - pitchDelta - rollDelta;
                            break;
                        case PropellerScript.PropellerPosition.BackLeft:   
                            currRPM = currRPM + pitchDelta + rollDelta;
                            break;
                        case PropellerScript.PropellerPosition.BackRight:
                            currRPM = currRPM + pitchDelta - rollDelta;
                            break;
                        default:
                            break;
                    }     
                    // Adjust currRPM depending on the propeller rotation direction as well
                    int yawSign = (int)propellerScripts[i].GetPropellerRotation();
                    currRPM += yawDelta * yawSign;
                    currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
                    propellerScripts[i].ApplyPropellerForce(currRPM, airDensity, stabilizationFlightMode);
                }
                break;
            case FlightMode.Stabilized:
                for(int i = 0; i < propellerScripts.Length; i++)
                {
                    float currRPM = hoverRPM + throttleAxis * (maxRPM - hoverRPM);
                    currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
                    propellerScripts[i].ApplyPropellerForce(currRPM, airDensity, stabilizationFlightMode);
                }
                //Simplified version of drone rotation for stabilized mode without 4 propellers changing torque, but just static limited rotation
                Quaternion targetRotation = Quaternion.Euler(pitchAxis * maxTiltAngle.x, transform.eulerAngles.y, -rollAxis * maxTiltAngle.z);
                // Debug.Log("Target rotation: " + targetRotation + " Pitch: " + pitchAxis + "Roll: " + rollAxis);
                //Slerp is better than lerp for this case, because it simulates the rotation in a more natural curvey instead of lerp which is more linear 
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltStabilizedRotationMultiplier * Time.fixedDeltaTime);
                //rotation for yaw is separate because it should not be affected by the tilt of the drone for stabilized mode, so it is applied on the world y axis
                transform.Rotate(Vector3.up, yawAxis * yawStabilizedRotationMultiplier * Time.fixedDeltaTime, Space.World);
                break;
            default:
                Debug.LogWarning("Unknown flight mode: " + stabilizationFlightMode);
                break;
        }
    }

    void CalculateVoltageDrop()
    {
        // total current draw is also affected by the throttle input, clamped so it changes based on throttle, not entirely realistic though because this change should not be linear (higher throttle can lead to even higher current draw)
        float batteryCurrentDraw = averageBatteryCurrentDraw * Mathf.Clamp(1 + throttleAxis, 0.5f, 1.5f);
        currBatteryChargeWithSafetyLimit -= batteryCurrentDraw * 1000f * (Time.fixedDeltaTime/3600f); 
        currBatteryChargeWithSafetyLimit = Mathf.Clamp(currBatteryChargeWithSafetyLimit, 0, batteryCapacity);
        currBatteryChargeOverall -= batteryCurrentDraw * 1000f * (Time.fixedDeltaTime/3600f);
        currBatteryChargeOverall = Mathf.Clamp(currBatteryChargeOverall, 0, batteryCapacity);
        //the currBatteryCharge will also affect the internal resistance of the battery, with added resistance that increases as the battery runs out of charge
        float voltageDrop = batteryCurrentDraw * internalBatteryResistance * CalculateIncreaseInBatteryResistance(); // Ohm's law
        // Debug.Log("Voltage drop: " + voltageDrop + "V" + " Total current draw: " + batteryCurrentDraw + "A" + " Average current draw per propeller: " + averageCellCurrentDraw + "A");
        ApplyVoltageDropToMaxRPM(voltageDrop);
    }

    //changes maxRPM and currBatteryVoltage, so the overall power of the drone will be lower because of the voltage drop  
    void ApplyVoltageDropToMaxRPM(float _voltageDrop)
    {
        currBatteryVoltage = maxCellVoltage * batteryCells - _voltageDrop;
        Debug.Log("Current battery voltage: " + currBatteryVoltage + "V" + " Voltage drop: " + _voltageDrop + "V");
        currBatteryVoltage = Mathf.Clamp(currBatteryVoltage, 0, maxCellVoltage * batteryCells);
        maxRPM = KV * currBatteryVoltage * propellerLoadedEfficiency/100f; // The voltage drop reduces the effective voltage available to the motors, which in turn reduces the maximum RPM they can achieve.
    }

    void CheckCellVoltage()
    {
        if (currBatteryVoltage / batteryCells <= batteryWarningCellVoltage)
        {
            Debug.LogWarning("Battery cell voltage is low: " + (currBatteryVoltage / batteryCells) + "V per cell. Please land the drone soon.");
        }
    }

    //safety battery percentage limit is for setting a warning for the user
    public float GetBatteryPercentageWithBatterySafety()
    {
        return (currBatteryChargeWithSafetyLimit / (batteryCapacity * (batteryMaxDischargePercentage / 100f))) * 100f;
    }

    //overall battery percentage without the safety limit for calculating internal battery resistance
    public float GetBatteryPercentageOverall()
    {
        return (currBatteryChargeOverall / batteryCapacity) * 100f;
    }

    //calculates increase in battery resistance based on the remaining battery charge, because the lower the charge the higher the internal resistance
    //change the values based on testing
    //also include temperature in the future, because it also affects it
    float CalculateIncreaseInBatteryResistance()
    {
        switch(GetBatteryPercentageOverall())
        {
            case 100f:
                return 1f;
            case > 90f:
                return 1.10f;
            case > 80f:
                return 1.15f; 
            case > 70f:
                return 1.20f;
            case > 60f:
                return 1.25f; 
            case > 50f:
                return 1.50f;
            case > 40f:
                return 1.75f; 
            case > 30f:
                return 2f;
            case > 20f:
                return 2.5f; 
            case > 10f:
                return 3f; 
            default:
                return 1f; 
        }
    }

}
