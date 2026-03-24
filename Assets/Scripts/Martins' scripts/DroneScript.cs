using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DroneScript : MonoBehaviour
{
    [Tooltip("Array of the drone propellers")]
    [SerializeField] 
    GameObject[] propellers;
    PropellerScript[] propellerScripts;

    [Header("Controls")]
    [SerializeField] 
    InputActionReference flyWASD;

    [SerializeField] 
    InputActionReference flyArrows;

    [Header("Environment")]
    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    private float airDensity = 1.225f;

    [Header("RPM Settings")]
    [Tooltip("Change in RPM. Per 1 propeller.")]
    [SerializeField]
    float deltaRPM;
    
    [Tooltip("RPM at which the drone is idle, so when throttle is all the way down. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float minRPMPercentage;

    [Tooltip("RPM at which the drone hovers when throttle stick is at 50%. In Percentage of max RPM. Per 1 propeller.")]
    [SerializeField]
    float hoverRPMPercentage;

    [Tooltip("Maximum RPM the drone propeller can reach. Per 1 propeller.")]
    [SerializeField]
    float maxRPM;

    [Header("Stabilization")]
    [SerializeField]
    private bool stabilizationFlightMode; 
    [Tooltip("Maximum pitch/roll angle for the propeller.")]
    [SerializeField]
    Vector3 maxTiltAngle;
    Vector3 currRotation;
    [SerializeField] PID rollPID;
    [SerializeField] PID pitchPID;
    [SerializeField] PID yawPID;
    Rigidbody droneRigidbody;

    float throttleAxis;
    float pitchAxis;
    float yawAxis;
    float rollAxis;
    float minRPM;
    float hoverRPM;
    
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
    }
    void Update()
    {
        Vector2 flyWASDInput = flyWASD.action.ReadValue<Vector2>();
        throttleAxis = flyWASDInput.y; 
        yawAxis = flyWASDInput.x;     
        Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
        rollAxis = arrowInput.x;
        pitchAxis = arrowInput.y;
    }

    //Logic for applying forces to the drone based on the player input
    void FixedUpdate()
    {
        float fullRPM = 0f;
        float pitchDelta = 0;
        float rollDelta = 0;
        float yawDelta = 0;
        if(stabilizationFlightMode)
        {
            //non functioning stabilization code, needs to be fixed
            currRotation = transform.eulerAngles;
            float currRoll = NormalizeAngle(currRotation.z);
            float currPitch = NormalizeAngle(currRotation.x);
            //float currYaw = NormalizeAngle(currRotation.y);
            float targetRoll = rollAxis * maxTiltAngle.z;
            float targetPitch = pitchAxis * maxTiltAngle.x;
            //float targetYaw = yawAxis * maxTiltAngle.y;
            float rollError = Mathf.DeltaAngle(currRoll, targetRoll);
            float pitchError = Mathf.DeltaAngle(currPitch, targetPitch);
            //float yawError = Mathf.DeltaAngle(currYaw, targetYaw);
            float rollCorrection = rollPID.GetValue(rollError, Time.fixedDeltaTime);
            float pitchCorrection = pitchPID.GetValue(pitchError, Time.fixedDeltaTime);
            //float yawCorrection = yawPID.GetValue(yawError, Time.fixedDeltaTime);
            if (Mathf.Abs(rollError) < 1f) rollCorrection = 0;
            if (Mathf.Abs(pitchError) < 1f) pitchCorrection = 0;
            //if (Mathf.Abs(yawError) < 1f) yawCorrection = 0;
            rollCorrection = Mathf.Clamp(rollCorrection, -1f, 1f);
            pitchCorrection = Mathf.Clamp(pitchCorrection, -1f, 1f);
            //yawCorrection = Mathf.Clamp(yawCorrection, -1f, 1f);
            pitchDelta = pitchCorrection * deltaRPM;
            rollDelta = rollCorrection * deltaRPM;
            //yawDelta = yawCorrection * deltaRPM;
            yawDelta = yawAxis * deltaRPM;
            Debug.Log(pitchDelta + " " + rollDelta + " " + yawDelta);
        }
        else
        {
            pitchDelta = pitchAxis * deltaRPM;
            rollDelta = rollAxis * deltaRPM;
            yawDelta = yawAxis * deltaRPM;
        }
        print("Pitch Delta: " + pitchDelta + " Roll Delta: " + rollDelta + " Yaw Delta: " + yawDelta);
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
            propellerScripts[i].ApplyPropellerForce(currRPM, airDensity);
            fullRPM += currRPM;
        }
        print("Full RPM after applying forces: " + fullRPM); 
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
