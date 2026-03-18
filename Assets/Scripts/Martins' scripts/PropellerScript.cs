using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

public class PropellerScript: MonoBehaviour
{
    public enum PropellerPosition { FrontLeft, FrontRight, BackLeft, BackRight }

    public enum RotationDirection { Clockwise = -1, CounterClockwise = 1 }
    
    [Tooltip("Position of the propeller on the drone. Options: FrontLeft, FrontRight, BackLeft, BackRight")]
    [SerializeField] 
    private PropellerPosition position;

    [Tooltip("Direction of rotation for the propeller. Options: Clockwise, CounterClockwise")]
    [SerializeField] 
    private RotationDirection propellerRotation;

    [SerializeField]
    float currRPM;

    [SerializeField]
    float maxPitchAngle;

    [SerializeField]
    float thrustCoefficient;

    [SerializeField]
    float torqueCoefficient;

    [SerializeField]
    float torqueMultiplier;

    [SerializeField]
    float propellerDiameter;

    Transform propellerTransform;

    Rigidbody droneRigidbody;

    int yawSign;

    void Awake()
    {
        propellerTransform = transform;
        droneRigidbody = GetComponentInParent<Rigidbody>();
        yawSign = (int)propellerRotation;
    }
    
    void Start()
    {
        
    }

    public void ApplyPropellerForce(float _currRPM, float _airDensity)
    {        
        float thrust = thrustCoefficient * _airDensity * Mathf.Pow(_currRPM / 60f, 2) * Mathf.Pow(propellerDiameter, 4);
        droneRigidbody.AddForceAtPosition(propellerTransform.up * thrust, propellerTransform.position);
        float torqueStrength = thrust * propellerDiameter * torqueCoefficient * torqueMultiplier; 
        droneRigidbody.AddTorque(droneRigidbody.transform.up * torqueStrength * yawSign);
    }

    public PropellerPosition GetPropellerPosition()
    {
        return position;
    }

    public RotationDirection GetPropellerRotation()
    {
        return propellerRotation;
    }
}
