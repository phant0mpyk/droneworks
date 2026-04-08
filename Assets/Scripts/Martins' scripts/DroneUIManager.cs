using System;
using Unity.VisualScripting;
using UnityEngine;

public class DroneUIManager : MonoBehaviour
{

    [SerializeField]
    private FlightController droneScript;
    [SerializeField]
    private DroneBatteryManager batteryScript;
    [SerializeField] private TMPro.TextMeshProUGUI batteryCellVoltageText;
    [SerializeField] private TMPro.TextMeshProUGUI timePassedText;
    float timePassed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        droneScript = GetComponent<FlightController>();
    }

    // Update is called once per frame
    void Update()
    {
        batteryCellVoltageText.text = "Battery Percentage: " +  batteryScript.GetBatteryPercentageOverall() + "% (" + batteryScript.currBatteryVoltage/14 + "V per cell)"; 
        timePassed += Time.deltaTime;
        timePassedText.text = string.Format("{0:N2}", timePassed);
    }
}
