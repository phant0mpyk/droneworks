using UnityEngine;
using UnityEngine.InputSystem;

public class CargoScript : MonoBehaviour
{
    [Header("Cargo Physical Settings")]
    public float cargoMass = 2.0f;
    [Tooltip("Offset relative to drone. Y should be negative to hang below.")]
    public Vector3 pickupOffset = new Vector3(0, -1.2f, 0);

    [Header("Input Settings")]
    public InputActionProperty dropAction;

    private Rigidbody cargoRb;
    private Rigidbody droneRb;
    private ConfigurableJoint joint;
    public bool isAttached = false; 
    private float originalDroneMass;
    private Collider cargoPhysicalCollider;

    void Awake()
    {
        cargoRb = GetComponent<Rigidbody>();
        foreach (var col in GetComponents<Collider>())
        {
            if (!col.isTrigger) cargoPhysicalCollider = col;
        }
    }

    void OnEnable() => dropAction.action.Enable();

    void Update()
    {
        if (isAttached && dropAction.action.WasPressedThisFrame())
        {
            DetachFromDrone();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && (other.CompareTag("Player") || other.GetComponent<DroneScript>()))
        {
            AttachToDrone(targetRb);
        }
    }

    void AttachToDrone(Rigidbody targetDroneRb)
    {
        isAttached = true;
        droneRb = targetDroneRb;

        originalDroneMass = droneRb.mass;
        droneRb.mass += cargoMass;

        transform.position = droneRb.transform.TransformPoint(pickupOffset);
        transform.rotation = droneRb.transform.rotation;

        joint = droneRb.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = cargoRb;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit swingLimit = new SoftJointLimit();
        swingLimit.limit = 15f;
        joint.highAngularXLimit = swingLimit;
        joint.angularYLimit = swingLimit;
        joint.angularZLimit = swingLimit;

        cargoRb.useGravity = true;
        if (cargoPhysicalCollider != null)
        {
            Physics.IgnoreCollision(cargoPhysicalCollider, droneRb.GetComponent<Collider>(), true);
        }
    }

    public void DetachFromDrone()
    {
        if (joint != null) Destroy(joint);

        if (droneRb != null)
        {
            droneRb.mass = originalDroneMass;
            if (cargoPhysicalCollider != null)
                Physics.IgnoreCollision(cargoPhysicalCollider, droneRb.GetComponent<Collider>(), false);
        }

        isAttached = false;
        droneRb = null;
    }
}