using UnityEditor.Callbacks;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class DroneScript : MonoBehaviour
{
    [Tooltip("Array of the drones' propellers ordered: TopLeft, TopRight, BottomLeft, BottomRight")]
    [SerializeField] 
    GameObject[] propellers;

    PropellerScript[] propellerScripts;

    [SerializeField] 
    InputActionReference throttle;

    [SerializeField] 
    InputActionReference yaw;

    [SerializeField] 
    InputActionReference flyArrows;

    [Tooltip("Density of the air, standard is 1.225 kg/m^3 at sea level")]
    [SerializeField]
    static public float airDensity = 1.225f;

    [SerializeField]
    float deltaRPM;
    
    [SerializeField]
    float hoverRPM;

    [SerializeField]
    float maxRPM;

    Rigidbody droneRigidbody;

    float throttleAxis;

    float pitchAxis;

    float yawAxis;

    float rollAxis;
    
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
    }
    void Update()
    {
        throttleAxis = throttle.action.ReadValue<float>(); 
        yawAxis = yaw.action.ReadValue<float>();     
        Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
        rollAxis = arrowInput.x;
        pitchAxis = arrowInput.y;
        // print("Throttle: " + throttleAxis + " Yaw: " + yawAxis + " Pitch: " + pitchAxis + " Roll: " + rollAxis);
    }

    //logic for applying forces to the drone based on the player input
    void FixedUpdate()
    {
        float baseRPM = hoverRPM + throttleAxis * deltaRPM;
        float pitchDelta = pitchAxis * deltaRPM;
        float rollDelta = rollAxis * deltaRPM;
        float yawDelta = yawAxis * deltaRPM;
        for (int i = 0; i < propellerScripts.Length; i++)
        {
            float currRPM = baseRPM;
            // print("Curr RPM: " + currRPM);
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
            int yawSign = (int)propellerScripts[i].GetPropellerRotation();
            currRPM += yawDelta * yawSign;
            currRPM = Mathf.Clamp(currRPM, 0f, maxRPM);
            propellerScripts[i].ApplyPropellerForce(currRPM, airDensity);
            // print("Curr RPM after applying forces: " + currRPM);   
        }
    }
}
