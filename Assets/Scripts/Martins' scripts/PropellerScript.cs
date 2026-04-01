using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PropellerScript: MonoBehaviour
{
    public enum PropellerPosition { FrontLeft, FrontRight, BackLeft, BackRight }
    public enum RotationDirection { Clockwise = -1, CounterClockwise = 1 }
    
    [Tooltip("Position of the propeller on the drone.")]
    [SerializeField] 
    private PropellerPosition position;

    [Tooltip("Direction of rotation for the propeller.")]
    [SerializeField] 
    private RotationDirection propellerRotation;

    [Tooltip("Thrust coefficient for the propeller.")]
    [SerializeField]
    float thrustCoefficient;

    [Tooltip("Torque coefficient for the propeller.")]
    [SerializeField]
    float torqueCoefficient;

    [Tooltip("Torque multiplier for the propeller. Used to adjust the strength of the torque applied by the propeller because Unity physics moment.")]
    [SerializeField]
    float torqueMultiplier;

    [Tooltip("Diameter of the propeller.")]
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

    public void ApplyPropellerForce(float _currRPM, float _airDensity, DroneScript.FlightMode _flightMode)
    {
        float thrust = thrustCoefficient * _airDensity * Mathf.Pow(Mathf.Round(_currRPM) / 60f, 2) * Mathf.Pow(propellerDiameter, 4);
        float torqueStrength = thrust * propellerDiameter * torqueCoefficient * torqueMultiplier;
        if(droneRigidbody != null)
        {
            droneRigidbody.AddForceAtPosition(propellerTransform.up * thrust, propellerTransform.position);
            switch (_flightMode)
            {
                case DroneScript.FlightMode.Acrobatic:
                    droneRigidbody.AddTorque(droneRigidbody.transform.up * torqueStrength * yawSign);
                    break;
                case DroneScript.FlightMode.Stabilized:
                    // No additional torque applied in stabilized mode, because it already did it with the entire drone itself using transform rotation
                    break;
                default:
                    Debug.LogWarning("Unknown flight mode: " + _flightMode);
                    break;
            }
        }else{
            Debug.LogError("Drone Rigidbody not found for propeller: " + position);
        }
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
