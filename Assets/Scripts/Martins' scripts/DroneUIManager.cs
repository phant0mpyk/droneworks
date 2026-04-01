using Unity.VisualScripting;
using UnityEngine;

public class DroneUIManager : MonoBehaviour
{

    private DroneScript droneScript;
    [SerializeField] private TMPro.TextMeshProUGUI batteryCellVoltageText;
    [SerializeField] private TMPro.TextMeshProUGUI flightTimeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        droneScript = GetComponent<DroneScript>();
    }

    // Update is called once per frame
    void Update()
    {
        batteryCellVoltageText.text = "Battery Voltage: " + droneScript.GetBatteryPercentageWithBatterySafety() + "% (" + droneScript.currBatteryVoltage/3 + "V per cell)"; 
        // flightTimeText.text = "Remaining Flight Time: " + droneScript.remainingFlightTimeMinutes;
    }
}
