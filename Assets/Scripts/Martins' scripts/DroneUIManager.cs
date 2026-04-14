using UnityEngine;
using TMPro;

public class DroneUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FlightController droneScript;
    [SerializeField] private DroneBatteryManager batteryScript;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI batteryCellVoltageText;
    [SerializeField] private TextMeshProUGUI timePassedText;

    [Header("Crosshair & Horizon")]
    [SerializeField] private RectTransform horizonLine;
    [SerializeField] private float pitchSensitivity = 5f;

    [Header("Cargo Settings")]
    [SerializeField] private TMPro.TextMeshProUGUI cargoWeightText;
    private CargoScript attachedCargo;

    [Header("Altitude Readouts")]
    [SerializeField] private TextMeshProUGUI altitudeASLText;
    [SerializeField] private TextMeshProUGUI altitudeAGLText;
    [SerializeField] private LayerMask groundLayer;

    [Header("Compass Settings")]
    [SerializeField] private RectTransform compassDisk;
    [SerializeField] private TextMeshProUGUI headingText;

    [Header("Flight Data")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Rigidbody droneRigidbody;

    private float _timePassed;

    void Start()
    {
        if (droneScript == null) droneScript = GetComponent<FlightController>();
    }

    void Update()
    {
        UpdateBatteryUI();
        UpdateFlightTimer();
        UpdateArtificialHorizon();
        UpdateCargoUI();
        UpdateAltitudeUI();
        UpdateCompass();
        UpdateSpeedUI();
    }
    void UpdateSpeedUI()
    {
        if (droneRigidbody == null) return;

        Vector3 velocity = droneRigidbody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float groundSpeed = horizontalVelocity.magnitude;
        float kmh = groundSpeed * 3.6f;

        if (speedText != null)
        {
            speedText.text = $"SPD: {kmh:F1} km/h";
        }
    }
    void UpdateAltitudeUI()
    {
        float asl = droneScript.transform.position.y;
        altitudeASLText.text = $"ALT (ASL): {asl:F1} m";

        float agl = 0;
        RaycastHit hit;
        if (Physics.Raycast(droneScript.transform.position, Vector3.down, out hit, 2000f, groundLayer))
        {
            agl = hit.distance;
        }

        altitudeAGLText.text = $"HGT (AGL): {agl:F1} m";
    }
    void UpdateBatteryUI()
    {
        float vPerCell = batteryScript.currBatteryVoltage / 14f;
        int percentage = Mathf.RoundToInt(batteryScript.GetBatteryPercentageOverall());
        batteryCellVoltageText.text = $"{percentage}% ({vPerCell:F2}V per cell)";
    }

    void UpdateCompass()
    {
        if (compassDisk == null) return;
        float heading = droneScript.transform.eulerAngles.y;
        compassDisk.localRotation = Quaternion.Euler(0, 0, heading);
        if (headingText != null)
        {
            headingText.text = $"{Mathf.RoundToInt(heading)}°";
        }
    }

        void UpdateCargoUI()
    {
        if (CargoScript.Instance != null && CargoScript.Instance.isAttached)
        {
            float weight = CargoScript.Instance.cargoMass;
            cargoWeightText.text = $"{weight:F2} kg";
            cargoWeightText.color = Color.white;
        }
        else
        {
            cargoWeightText.text = "0.00 kg";
            cargoWeightText.color = new Color(1, 1, 1, 0.5f);
        }
    }

    void UpdateFlightTimer()
    {
        _timePassed += Time.deltaTime;
        System.TimeSpan t = System.TimeSpan.FromSeconds(_timePassed);
        timePassedText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    void UpdateArtificialHorizon()
    {
        if (horizonLine == null) return;
        Vector3 rotation = droneScript.transform.eulerAngles;
        float roll = rotation.z;
        float pitch = rotation.x;
        if (pitch > 180) pitch -= 360;
        horizonLine.localRotation = Quaternion.Euler(0, 0, -roll);
        float yOffset = pitch * pitchSensitivity;
        horizonLine.anchoredPosition = new Vector2(0, -yOffset);
    }
}