using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private Transform droneCamera;

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

    [Header("Death Screen")]
    [SerializeField] private GameObject deathScreen;

    [Header("Flight Limits")]
    [SerializeField] private CanvasGroup warningCanvasGroup;
    [SerializeField] private float maxFlightHeight = 120f;
    [SerializeField] private float fadeSpeed = 2f;

    private bool droneDestroyed = false;

    private bool spawnedDeathScreen = false;

    private float _timePassed;

    void Start()
    {
        if (droneScript == null) droneScript = GetComponent<FlightController>();
    }

    void Update()
    {
        droneDestroyed = droneScript.GetDroneDestroyed();
        //makes sure that the rest of the code only runs when the drone is alive and that spawning of death screen is only done once
        if (droneDestroyed)
        {
            if (!spawnedDeathScreen)
            {
                spawnedDeathScreen = true;
                DestroyDroneUI();
            }
            return;
        }
        spawnedDeathScreen = false;
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
        else
        {
            agl = asl;
        }
        droneScript.SetCurrentAGL(agl);
        altitudeAGLText.text = $"HGT (AGL): {agl:F1} m";

        if (warningCanvasGroup != null)
        {
            float targetAlpha = (agl >= maxFlightHeight) ? 1.0f : 0.0f;
            warningCanvasGroup.alpha = Mathf.MoveTowards(
                warningCanvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );
            altitudeAGLText.color = Color.Lerp(Color.white, Color.red, warningCanvasGroup.alpha);
        }
    }

    void UpdateBatteryUI()
    {
        float vPerCell = batteryScript.currBatteryVoltage / batteryScript.GetBatteryCells();
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
            headingText.text = $"{Mathf.RoundToInt(heading)}*";
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
        if (horizonLine == null || droneCamera == null) return;
        float roll = droneScript.transform.eulerAngles.z;
        float dronePitch = droneScript.transform.eulerAngles.x;
        if (dronePitch > 180) dronePitch -= 360;
        float cameraLocalPitch = droneCamera.localEulerAngles.x;
        if (cameraLocalPitch > 180) cameraLocalPitch -= 360;
        float adjustedPitch = dronePitch + cameraLocalPitch;
        horizonLine.localRotation = Quaternion.Euler(0, 0, -roll);
        float yOffset = adjustedPitch * pitchSensitivity;
        horizonLine.anchoredPosition = new Vector2(0, yOffset);
    }

    //spawns Alex's shader glitch effect
    public void DestroyDroneUI()
    {
        Instantiate(deathScreen, transform.position, Quaternion.identity);
    }
}