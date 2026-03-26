using Unity.Mathematics;
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
    [Header("Controls")]
    [SerializeField] 
    InputActionReference flyWASD;

    [SerializeField] 
    InputActionReference flyArrows;

    [SerializeField]
    InputActionReference leftStickInputAxis;

    [SerializeField]
    InputActionReference rightStickInputAxis;

    [SerializeField]
    Camera droneCamera;

    [Tooltip("Camera up-tilt in degrees. 0-10 is standard for cinematic drones, 10-25 for FPV freestyle drones, above 25 for racing drones.")]
    [SerializeField]
    float cameraTilt = 0f;

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;

    enum FlightMode { Stabilized, Acrobatic }

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
        minRPM = minRPMPercentage/100 * maxRPM;
        hoverRPM = hoverRPMPercentage/100 * maxRPM;
        minThrust = minThrustPercentage/100 * maxThrust;
        hoverThrust = hoverThrustPercentage/100 * maxThrust;
    }
    void Update()
    {
        Vector2 flyLeftStickInput = leftStickInputAxis.action.ReadValue<Vector2>();
        Vector2 flyRightStickInput = rightStickInputAxis.action.ReadValue<Vector2>();
        throttleAxis = flyLeftStickInput.y;
        yawAxis = flyLeftStickInput.x;
        pitchAxis = flyRightStickInput.y;
        rollAxis = flyRightStickInput.x;
        Debug.Log(yawAxis +" " + pitchAxis +  " " + rollAxis + " " + throttleAxis);
        //for keyboard, implement input switching later
        // Vector2 flyWASDInput = flyWASD.action.ReadValue<Vector2>();
        // throttleAxis = flyWASDInput.y; 
        // yawAxis = flyWASDInput.x;     
        // Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
        // rollAxis = arrowInput.x;
        // pitchAxis = arrowInput.y;
        droneCamera.transform.localRotation = Quaternion.Euler(-cameraTilt, 0f, 0f);
    }

    //Applies thrust and rotations based on the current flight mode. 
    //Acrobatic flight mode calculates thrust realistically based on RPM on all 4 propellers and applies it with physics
    //Stabilized flight mode applies simplified thrust and rotates it using transform instead if torque
    void FixedUpdate()
    {
        switch (stabilizationFlightMode)
        {
            case FlightMode.Stabilized:
                StabilizedFlightPhysics();
                break;
            case FlightMode.Acrobatic:
                AcrobaticFlightPhysics();
                break;
        }
    }

    //Simplified version of drone physics without 4 propellers, just one thrust
    //The rotation is manipulated with transform over physics like in acrobatic flight mode because the previous version with PID and propeller RPM/thrust was not working correctly, most likely because of tiny errors and PID being too aggresive
    //Also the previous version was working with tilt/yaw but roll/combining of rotations caused a spinout
    void StabilizedFlightPhysics()
    {
        float finalThrust = 0f;
        Quaternion targetRotation = Quaternion.Euler(pitchAxis * maxTiltAngle.x, transform.eulerAngles.y, -rollAxis * maxTiltAngle.z);
        Debug.Log("Target rotation: " + targetRotation + " Pitch: " + pitchAxis + "Roll: " + rollAxis);
        //Slerp is better than lerp for this case, because it simulates the rotation in a more natural curvey instead of lerp which is more linear 
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltStabilizedRotationMultiplier * Time.fixedDeltaTime);
        //rotation for yaw is separate because it should not be affected by the tilt of the drone for stabilized mode, so it is applied on the world y axis
        transform.Rotate(Vector3.up, yawAxis * yawStabilizedRotationMultiplier * Time.fixedDeltaTime, Space.World);
        for (int i = 0; i < propellerScripts.Length; i++)
        {
            float thrust = hoverThrust + throttleAxis * (maxThrust - hoverThrust);
            thrust = Mathf.Clamp(thrust, minThrust, maxThrust);
            propellerScripts[i].ApplyPropellerForceStabilized(thrust);
            finalThrust += thrust;
        }
        Debug.Log("Applying total stabilized thrust: " + finalThrust + "Max thrust: " + maxThrust * propellerScripts.Length);
    }

    void AcrobaticFlightPhysics()
    {
        float pitchDelta = 0f;
        float rollDelta = 0f;
        float yawDelta = 0f;
        pitchDelta = pitchAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        rollDelta = rollAxis * deltaRPM * tiltAcrobaticRotationMultiplier;
        yawDelta = yawAxis * deltaRPM * yawAcrobaticRotationMultiplier;
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
            propellerScripts[i].ApplyPropellerForceAcrobatic(currRPM, airDensity);
        }
    }
}

