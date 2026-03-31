using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;


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

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;
    float currAltitude = 0f;

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
    float batteryDischargePercentage = 80f;

    [Tooltip("Average current draw in Amps. Used to calculate battery draining. Per 1 propeller.")]
    [SerializeField]
    float averageCurrentDraw = 0f;

    [Tooltip("For a battery warning at a certain Voltage. Per 1 cell.")]
    [SerializeField]
    float batteryWarningCellVoltage = 3.5f;

    public float currBatteryVoltage { get; private set; }

    //calculating this based off of
    [Tooltip("Estimated maximum flight time in minutes. Required to calculate the battery voltage drop during flight. It's an estimate that can be found on the internet per drone.")]
    [SerializeField]
    float estimatedFlightTimeMinutes;

    [SerializeField]
    float calculatedFlightTimeMinutes;

    public float remainingFlightTimeMinutes { get; private set; }

    public enum FlightMode { Stabilized, Acrobatic }

    [SerializeField]
    [Header("Flight Mode")]
    private FlightMode stabilizationFlightMode;

    [Header("Acrobatic Flight Mode Settings")]

    [Tooltip("Array of the drone propellers")]
    [SerializeField] 
    GameObject[] propellers;
    PropellerScript[] propellerScripts;

    [Tooltip("Rotation speed multiplier for tilting (roll/pitch) the drone for fine-tuning.")]
    [SerializeField]
    float tiltAcrobaticRotationMultiplier = 1f;

    [Tooltip("Rotation speed multiplier for yawing the drone for fine-tuning.")]
    [SerializeField]
    float yawAcrobaticRotationMultiplier = 1f;

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

    [Tooltip("Thrust at which the drone is idle, so when throttle is all the way down. In Percentage of max thrust. Per 1 propeller.")]
    [SerializeField]
    float minThrustPercentage;

    [Tooltip("Maximum possible thrust for the propeller. Per 1 propeller in Newtons.")]
    [SerializeField]
    float maxThrust;

    [Tooltip("Thrust at which the drone hovers when throttle stick is at 50%. In Percentage of max thrust. Per 1 propeller.")]
    [SerializeField]
    float hoverThrustPercentage;

    float minThrust;
    float hoverThrust;

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
        
        averageCurrentDraw = (batteryCapacity/1000f)/estimatedFlightTimeMinutes;
        remainingFlightTimeMinutes = estimatedFlightTimeMinutes;
        originalMaxRPM = maxRPM;
    }
    void Update()
    {
        if (droneActive)
        {
            remainingFlightTimeMinutes -= Time.deltaTime / 60f; // Convert seconds to minutes
            if (remainingFlightTimeMinutes <= 0 || currBatteryVoltage/batteryCells <= minCellVoltage)
            {
                remainingFlightTimeMinutes = 0;
                droneActive = false;
                Debug.Log("Battery is fully discharged.");
            }
            CheckBatteryVoltage();
            //update each frame cuz it needs to respond to the battery voltage dropping
            minRPM = minRPMPercentage/100 * maxRPM;
            hoverRPM = hoverRPMPercentage/100 * maxRPM;
            minThrust = minThrustPercentage/100 * maxThrust;
            hoverThrust = hoverThrustPercentage/100 * maxThrust;
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

    // //Simplified version of drone physics without 4 propellers, just one thrust
    // //The rotation is manipulated with transform over physics like in acrobatic flight mode because the previous version with PID and propeller RPM/thrust was not working correctly, most likely because of tiny errors and PID being too aggresive
    // //Also the previous version was working with tilt/yaw but roll/combining of rotations caused a spinout
    // void StabilizedFlightPhysics()
    // {
    //     // float finalThrust = 0f;
    //     Quaternion targetRotation = Quaternion.Euler(pitchAxis * maxTiltAngle.x, transform.eulerAngles.y, -rollAxis * maxTiltAngle.z);
    //     // Debug.Log("Target rotation: " + targetRotation + " Pitch: " + pitchAxis + "Roll: " + rollAxis);
    //     //Slerp is better than lerp for this case, because it simulates the rotation in a more natural curvey instead of lerp which is more linear 
    //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltStabilizedRotationMultiplier * Time.fixedDeltaTime);
    //     //rotation for yaw is separate because it should not be affected by the tilt of the drone for stabilized mode, so it is applied on the world y axis
    //     transform.Rotate(Vector3.up, yawAxis * yawStabilizedRotationMultiplier * Time.fixedDeltaTime, Space.World);
    //     for (int i = 0; i < propellerScripts.Length; i++)
    //     {
    //         float thrust = hoverThrust + throttleAxis * (maxThrust - hoverThrust);
    //         thrust = Mathf.Clamp(thrust, minThrust, maxThrust);
    //         propellerScripts[i].ApplyPropellerForceStabilized(thrust);
    //         // finalThrust += thrust;
    //     }
    //     // Debug.Log("Applying total stabilized thrust: " + finalThrust + "Max thrust: " + maxThrust * propellerScripts.Length);
    // }

    // void AcrobaticFlightPhysics()
    // {
    //     float pitchDelta = 0f;
    //     float rollDelta = 0f;
    //     float yawDelta = 0f;
    //     pitchDelta = pitchAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
    //     rollDelta = rollAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
    //     yawDelta = yawAxis * deltaRPM * yawAcrobaticRotationMultiplier;
    //     for (int i = 0; i < propellerScripts.Length; i++)
    //     {
    //         float currRPM = hoverRPM + throttleAxis * (maxRPM - hoverRPM);
    //         //Adjust currRPM based on propeller position
    //         switch (propellerScripts[i].GetPropellerPosition())
    //         {
    //             case PropellerScript.PropellerPosition.FrontLeft:
    //                 currRPM = currRPM - pitchDelta + rollDelta;                 
    //                 break;
    //             case PropellerScript.PropellerPosition.FrontRight:
    //                 currRPM = currRPM - pitchDelta - rollDelta;
    //                 break;
    //             case PropellerScript.PropellerPosition.BackLeft:   
    //                 currRPM = currRPM + pitchDelta + rollDelta;
    //                 break;
    //             case PropellerScript.PropellerPosition.BackRight:
    //                 currRPM = currRPM + pitchDelta - rollDelta;
    //                 break;
    //             default:
    //                 break;
    //         }     
    //         // Adjust currRPM depending on the propeller rotation direction as well
    //         int yawSign = (int)propellerScripts[i].GetPropellerRotation();
    //         currRPM += yawDelta * yawSign;
    //         currRPM = Mathf.Clamp(currRPM, minRPM, maxRPM);
            
    //     }
    // }


    //is calculated with a predetermined flight time that lowers the battery capacity
    void CalculateVoltageDrop()
    {
        float maxBatteryCapacity = batteryCapacity * (batteryDischargePercentage / 100f);
        Debug.Log("Max battery capacity for this flight: " + maxBatteryCapacity + "mAh");
        averageCurrentDraw = (batteryCapacity/1000f)/remainingFlightTimeMinutes; 
        Debug.Log("Average current draw per propeller: " + averageCurrentDraw + "A" + " Remaining flight time: " + remainingFlightTimeMinutes + " minutes");
        float totalCurrentDraw = averageCurrentDraw * propellerScripts.Length;
        calculatedFlightTimeMinutes = ((batteryCapacity/1000f) * batteryDischargePercentage) / totalCurrentDraw; 
        Debug.Log("Calculated flight time: " + calculatedFlightTimeMinutes + " minutes" + " Total current draw: " + totalCurrentDraw + "A");
        float voltageDrop = totalCurrentDraw * internalBatteryResistance; // Ohm's law
        Debug.Log("Voltage drop: " + voltageDrop + "V" + " Total current draw: " + totalCurrentDraw + "A" + " Average current draw per propeller: " + averageCurrentDraw + "A");
        ApplyVoltageDropToMaxRPM(voltageDrop);
    }

    // //for calculating C rate using battery specifications and current draw, converting from mAh to Ah and then to A, also taking into account the battery discharge percentage to get the usable capacity of the battery for the flight
    // void CalculateCRate()
    // {
    //     float maxBatteryCapacity = batteryCapacity * (batteryDischargePercentage / 100f);
    //     averageCurrentDraw = (batteryCapacity/1000f)/remainingFlightTimeMinutes; 
    //     float totalCurrentDraw = averageCurrentDraw * propellerScripts.Length;
    //     cRate = totalCurrentDraw / maxBatteryCapacity; // C rate is the ratio of the current draw to the battery capacity, so it gives an indication of how fast the battery is being discharged. A higher C rate means a faster discharge, which can lead to shorter flight times and potential damage to the battery if it exceeds its maximum C rating.
    // }

    //changes maxRPM, so the overall power of the drone will be lower because of the voltage drop  
    void ApplyVoltageDropToMaxRPM(float _voltageDrop)
    {
        currBatteryVoltage = (batteryCells * maxCellVoltage) - _voltageDrop;
        maxRPM = KV * currBatteryVoltage * propellerLoadedEfficiency/100f; // The voltage drop reduces the effective voltage available to the motors, which in turn reduces the maximum RPM they can achieve.
    }

    //checks the battery voltage for debugging purposes for now
    void CheckBatteryVoltage()
    {
        float currentCellVoltage = (batteryCells * maxCellVoltage) - (averageCurrentDraw * internalBatteryResistance);
        if (currentCellVoltage <= minCellVoltage)
        {
            Debug.Log("Battery voltage is critically low: " + currentCellVoltage + "V per cell. Please land the drone immediately.");
        }else if (currentCellVoltage <= batteryWarningCellVoltage)
        {
            Debug.Log("Battery voltage is low: " + currentCellVoltage + "V per cell. Please land the drone soon.");
        }
        Debug.Log("Current battery voltage: " + currentCellVoltage + "V per cell.");
    }
}
