using UnityEngine;

public class DroneBoundaryEnforcer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxFlightHeight = 120f;
    [SerializeField] private float warningDuration = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dependencies")]
    [SerializeField] private FlightController droneScript;
    [SerializeField] private Rigidbody droneRigidbody;

    [SerializeField] private SFXManager sfxManager;
    [SerializeField] private string heightWarningClipName = "LightningWarning";
    private bool _hasPlayedWarning = false;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _violationTimer = 0f;

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        if (droneScript == null) droneScript = GetComponent<FlightController>();
        if (droneRigidbody == null) droneRigidbody = GetComponent<Rigidbody>();
        if (sfxManager == null) sfxManager = FindFirstObjectByType<SFXManager>();
    }

    void Update()
    {
        float asl = transform.position.y;
        float agl = asl;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2000f, groundLayer))
        {
            agl = hit.distance;
        }

        if (agl >= maxFlightHeight)
        {
            _violationTimer += Time.deltaTime;
            if(sfxManager != null && !_hasPlayedWarning)
            {
                sfxManager.PlayVoicelineSFX(heightWarningClipName);
                _hasPlayedWarning = true;
            }
            if (_violationTimer >= warningDuration)
            {
                TeleportBackToStart();
            }
        }
        else
        {
            _violationTimer = 0f;
            _hasPlayedWarning = false;
        }
    }

    public void TeleportBackToStart()
    {
        Debug.Log("teleporting to start");

        if (droneRigidbody != null)
        {
            droneRigidbody.linearVelocity = Vector3.zero;
            droneRigidbody.angularVelocity = Vector3.zero;

            droneRigidbody.position = _startPosition;
            droneRigidbody.rotation = _startRotation;
        }
        else
        { 
            transform.position = _startPosition;
            transform.rotation = _startRotation;
        }

        _violationTimer = 0f;
    }
}