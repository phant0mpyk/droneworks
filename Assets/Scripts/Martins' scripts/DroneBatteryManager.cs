using UnityEngine;

public class DroneBatteryManager : MonoBehaviour
{
    [SerializeField]
    FlightController flightController;
    //the battery settings need to be set by the drone model individually, however flight time can be adjusted as needed
    [Header("Battery Settings")]
    [Tooltip("Battery capacity in mAh.")]
    [SerializeField]
    float batteryCapacity;

    [Tooltip("Internal resistance of the battery in Ohms. Won't include cable resistance, which can be significant for high current draw, but for simplicity it is not included in the current version of the script.")]
    [SerializeField]
    float internalBatteryResistance;

    [Tooltip("This is the RPM per volt of applied voltage. Used to calculate maxRPM with efficiency and battery voltage. This however doesn't take mass into account (measured on just the motor of that propeller), which is why it uses propellerLoadedEfficiency as well to calculate maxRPM representing the drone capability with load")]
    [SerializeField]
    float KV;

    [Tooltip("This is the efficiency of the motor and propeller combination. Used to calculate maxRPM, because the drone with load (such as the drone itself with camera) doesn't reach 100% efficiency. In percentage.")]
    [SerializeField]
    float propellerLoadedEfficiency;

    [Tooltip ("Number of cells in the battery. In Series(S).")]
    [SerializeField]
    int batteryCells;

    [Tooltip("Maximum voltage the battery has. Per 1 cell. Standard maximum is 4.2V for LiPo/LiIon batteries, although nominal voltage is usually 3.7V.")]
    [SerializeField]
    float maxCellVoltage = 4.2f;

    [Tooltip("Nominal voltage the battery has. Per 1 cell. Standard nominal voltage is 3.7V for LiPo/LiIon batteries.")]
    [SerializeField]
    float nominalCellVoltage = 3.7f;

    [Tooltip("Minimum voltage in Volts. The battery will shutdown at this value. Same for LiPo / LiIon batteries and standard is 3.0V. Per 1 cell.")]
    [SerializeField]
    float minCellVoltage = 3.0f;

    [Tooltip("Battery discharge that you allow during the flight. As batteries can be damaged if fully discharged, it's common practice never to discharge them by more than 80%. In percentage.")]
    [SerializeField]
    float batteryMaxDischargePercentage = 80f;

    [Tooltip("Average current draw in Amps. Used to calculate battery draining. Per 1 propeller.")]
    [SerializeField]
    float averageBatteryCurrentDraw = 0f;

    [Tooltip("For a battery warning at a certain Voltage. Per 1 cell.")]
    [SerializeField]
    float batteryWarningCellVoltage = 3.5f;

    [Tooltip("Maximum possible flight time of the drone in minutes. This is only an estimated value, because the battery can drain faster")]
    [SerializeField]
    float maxFlightTimeMinutes;

    float remainingFlightTimeMinutes;

    [SerializeField]
    float energy;

    [SerializeField]
    float currBatteryChargeWithSafetyLimit;

    [SerializeField]
    float currBatteryChargeOverall;

    public float currBatteryVoltage { get; private set; }

    bool batteryActive = false;

    void Awake()
    {
        flightController = GetComponentInParent<FlightController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ToggleDroneBattery(true);
        //calculates max battery charge based off of the dischargePercentage
        //irl this is set to conserve the battery, so it doesn't let you discharge it fully
        float usableBatteryCapacity = batteryCapacity * (batteryMaxDischargePercentage / 100f);
        currBatteryChargeWithSafetyLimit = usableBatteryCapacity;
        currBatteryChargeOverall = batteryCapacity;
        remainingFlightTimeMinutes = maxFlightTimeMinutes;

        //calculates the average current draw per propeller with power using predetermined energy, maxFlightTime and nominal cell voltage
        //before it wasn't calculated with power but from the battery capacity and flight time, which lead to the voltage drop not being realistic and dropping too fast
        //calculates the electrical power in Watts, that is produced by the drone with it's motors based on the maximum flight time it has. 
        float power = energy/(maxFlightTimeMinutes/60f);
        //average current draw formula per propeller with power and nominal cell voltage
        //could also be predefined so we wouldn't need to calculate it based on the max flight time, but this way we can adjust the flight time how we want it. Don't set it too low though.
        averageBatteryCurrentDraw = power/(nominalCellVoltage * flightController.GetPropellerCount());
        Debug.Log("Average current draw per propeller: " + averageBatteryCurrentDraw + "A");
    }

    // Update is called once per frame
    void Update()
    {
        CheckCellVoltage();
    }
    public float CalculateVoltageDrop(float _throttleAxis)
    {
        // total current draw is also affected by the throttle input, clamped so it changes based on throttle, not entirely realistic though because this change should not be linear (higher throttle can lead to even higher current draw)
        float batteryCurrentDraw = averageBatteryCurrentDraw * Mathf.Clamp(1 + _throttleAxis, 0.5f, 1.5f);
        currBatteryChargeWithSafetyLimit -= batteryCurrentDraw * 1000f * (Time.fixedDeltaTime/3600f); 
        currBatteryChargeWithSafetyLimit = Mathf.Clamp(currBatteryChargeWithSafetyLimit, 0, batteryCapacity);
        currBatteryChargeOverall -= batteryCurrentDraw * 1000f * (Time.fixedDeltaTime/3600f);
        currBatteryChargeOverall = Mathf.Clamp(currBatteryChargeOverall, 0, batteryCapacity);
        //the currBatteryCharge will also affect the internal resistance of the battery, with added resistance that increases as the battery runs out of charge
        float voltageDrop = batteryCurrentDraw * internalBatteryResistance * CalculateIncreaseInBatteryResistance(); // Ohm's law
        // Debug.Log("Voltage drop: " + voltageDrop + "V" + " Total current draw: " + batteryCurrentDraw + "A" + " Average current draw per propeller: " + averageCellCurrentDraw + "A");
        return voltageDrop;
    }

    //changes maxRPM and currBatteryVoltage, so the overall power of the drone will be lower because of the voltage drop  
    public float CalculateMaxRPMAfterVoltageDrop(float _voltageDrop)
    {
        currBatteryVoltage = maxCellVoltage * batteryCells - _voltageDrop;
        Debug.Log("Current battery voltage: " + currBatteryVoltage + "V" + " Voltage drop: " + _voltageDrop + "V");
        currBatteryVoltage = Mathf.Clamp(currBatteryVoltage, 0, maxCellVoltage * batteryCells);
        float maxRPM = KV * currBatteryVoltage * propellerLoadedEfficiency/100f; // The voltage drop reduces the effective voltage available to the motors, which in turn reduces the maximum RPM they can achieve.
        return maxRPM;
    }

    void CheckCellVoltage()
    {
        if (currBatteryVoltage / batteryCells <= batteryWarningCellVoltage)
        {
            Debug.LogWarning("Battery cell voltage is low: " + (currBatteryVoltage / batteryCells) + "V per cell. Please land the drone soon.");
        }
    }

    //safety battery percentage limit is for setting a warning for the user
    public float GetBatteryPercentageWithBatterySafety()
    {
        return (currBatteryChargeWithSafetyLimit / (batteryCapacity * (batteryMaxDischargePercentage / 100f))) * 100f;
    }

    //overall battery percentage without the safety limit for calculating internal battery resistance
    public float GetBatteryPercentageOverall()
    {
        return (currBatteryChargeOverall / batteryCapacity) * 100f;
    }

    //calculates increase in battery resistance based on the remaining battery charge, because the lower the charge the higher the internal resistance
    //change the values based on testing
    //also include temperature and wind applied in the future, because it also affects it
    float CalculateIncreaseInBatteryResistance()
    {
        switch(GetBatteryPercentageOverall())
        {
            case 100f:
                return 1f;
            case > 90f:
                return 1.10f;
            case > 80f:
                return 1.15f; 
            case > 70f:
                return 1.20f;
            case > 60f:
                return 1.25f; 
            case > 50f:
                return 1.50f;
            case > 40f:
                return 1.75f; 
            case > 30f:
                return 2f;
            case > 20f:
                return 2.5f; 
            case > 10f:
                return 3f; 
            default:
                return 1f; 
        }
    }

    public void ToggleDroneBattery(bool _batteryActive)
    {
        batteryActive = _batteryActive;
    }

    public bool GetDroneBattery()
    {
        return batteryActive;
    }
}
