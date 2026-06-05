using UnityEngine;
using UnityEngine.InputSystem;

public class DroneInputManager : MonoBehaviour
{
    [SerializeField]
    FlightController flightController;

    float throttleAxis;
    float pitchAxis;
    float yawAxis;
    float rollAxis;
    private bool isArmed = false;
    public bool IsArmed => isArmed;

    [Header("Arming References")]
    [SerializeField] InputActionReference armActionRef;
    [SerializeField] TMPro.TextMeshProUGUI disarmedTextUI;

    [Header("Core Flight Axis References")]
    [SerializeField] InputActionReference throttleActionRef;
    [SerializeField] InputActionReference yawActionRef;
    [SerializeField] InputActionReference pitchActionRef;
    [SerializeField] InputActionReference rollActionRef;

    [Header("Other Inputs")]
    [SerializeField] InputActionReference toggleCameraKeyboard;
    [SerializeField] InputActionReference toggleCameraController;
    [SerializeField] InputActionReference rotateKeyboardGimbalUp;
    [SerializeField] InputActionReference rotateKeyboardGimbalDown;
    [SerializeField] InputActionReference rotateControllerGimbalUp;
    [SerializeField] InputActionReference rotateControllerGimbalDown;
    [SerializeField] InputActionReference toggleKeyboardThermalVision;
    [SerializeField] InputActionReference toggleControllerThermalVision;
    [SerializeField] InputActionReference pingKeyboard;
    [SerializeField] InputActionReference pingController;
    [SerializeField] InputActionReference rotateControllerGimbal3Way;

    bool keyboardInputActive = false;
    bool controllerInputActive = false;
    private bool forceManualInputSelection = false;
    private int manualInputChoice = 0;

    void Start()
    {
        if (rotateControllerGimbal3Way != null) rotateControllerGimbal3Way.action.Enable();
        if (armActionRef != null) armActionRef.action.Enable();

        // 1. Enable our 4 core flight axes
        if (throttleActionRef != null) throttleActionRef.action.Enable();
        if (yawActionRef != null) yawActionRef.action.Enable();
        if (pitchActionRef != null) pitchActionRef.action.Enable();
        if (rollActionRef != null) rollActionRef.action.Enable();

        // 2. Enable peripheral utilities
        toggleCameraKeyboard.action.Enable();
        toggleCameraController.action.Enable();
        rotateControllerGimbalUp.action.Enable();
        rotateControllerGimbalDown.action.Enable();
        rotateKeyboardGimbalUp.action.Enable();
        rotateKeyboardGimbalDown.action.Enable();
        toggleKeyboardThermalVision.action.Enable();
        toggleControllerThermalVision.action.Enable();
        pingKeyboard.action.Enable();
        pingController.action.Enable();

        // Subscriptions
        toggleKeyboardThermalVision.action.performed += ToggleThermalVision;
        toggleControllerThermalVision.action.performed += ToggleThermalVision;
        toggleCameraController.action.performed += CameraToggle;
        toggleCameraKeyboard.action.performed += CameraToggle;

        flightController.SetDroneArmed(isArmed);
    }

    void Update()
    {
        CheckArmStatus();
        DecideInputMethod();
        ReadInput();
    }

    void CheckArmStatus()
    {
        if(flightController.GetDroneDestroyed())
        {
            isArmed = false;
            disarmedTextUI.gameObject.SetActive(!isArmed);
            return;
        }
        if (armActionRef == null || armActionRef.action == null) return;
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            isArmed = !isArmed;
            if (disarmedTextUI != null)
                disarmedTextUI.gameObject.SetActive(!isArmed);
                flightController.SetDroneArmed(isArmed);
            return;
        }
        if (controllerInputActive)
        {
            float switchValue = armActionRef.action.ReadValue<float>();
            isArmed = Mathf.Approximately(switchValue, 1f);

            if (disarmedTextUI != null)
            {
                disarmedTextUI.gameObject.SetActive(!isArmed);
                flightController.SetDroneArmed(isArmed);
            }
        }
    }

    void DecideInputMethod()
    {
        if (forceManualInputSelection)
        {
            keyboardInputActive = (manualInputChoice == 0);
            controllerInputActive = (manualInputChoice == 1);
            return;
        }

        if (throttleActionRef.action.triggered || yawActionRef.action.triggered ||
            pitchActionRef.action.triggered || rollActionRef.action.triggered)
        {
            var activeControl = throttleActionRef.action.activeControl;
            if (activeControl != null)
            {
                if (activeControl.device is Keyboard)
                {
                    controllerInputActive = false;
                    keyboardInputActive = true;
                }
                else
                {
                    controllerInputActive = true;
                    keyboardInputActive = false;
                }
            }
        }
    }

    void ReadInput()
    {
        if (!isArmed)
        {
            throttleAxis = 0f;
            yawAxis = 0f;
            pitchAxis = 0f;
            rollAxis = 0f;
            return;
        }
            throttleAxis = throttleActionRef.action.ReadValue<float>();
            yawAxis = yawActionRef.action.ReadValue<float>();
            pitchAxis = pitchActionRef.action.ReadValue<float>();
        if (keyboardInputActive)
        {
            rollAxis = -rollActionRef.action.ReadValue<float>();
        }else if(controllerInputActive)
        {
            rollAxis = rollActionRef.action.ReadValue<float>();
        }
        GimbalRotation();
    }

    public float GetThrottleAxis() => throttleAxis;
    public float GetYawAxis() => yawAxis;
    public float GetPitchAxis() => pitchAxis;
    public float GetRollAxis() => rollAxis;

    private void CameraToggle(InputAction.CallbackContext context) => flightController.OnCameraToggle();
    private void Ping(InputAction.CallbackContext context) => flightController.Ping();
    private void ToggleThermalVision(InputAction.CallbackContext context) => flightController.ToggleThermalVision();

    private void GimbalRotation()
    {
        if (keyboardInputActive)
        {
            if (rotateKeyboardGimbalUp.action.IsPressed())
            {
                Debug.Log("Keyboard UP");
                flightController.RotateGimbalUp();
            }
            else if (rotateKeyboardGimbalDown.action.IsPressed())
            {
                Debug.Log("Keyboard Down");
                flightController.RotateGimbalDown();
            }
        }
        else if (controllerInputActive && rotateControllerGimbal3Way != null)
        {
            float switchValue = rotateControllerGimbal3Way.action.ReadValue<float>();

            if (switchValue > 0.5f)
            {
                Debug.Log("3-Way Switch UP");
                flightController.RotateGimbalUp();
            }
            else if (switchValue < -0.5f)
            {
                Debug.Log("3-Way Switch DOWN");
                flightController.RotateGimbalDown();
            }
        }
    }

    public void SetInputMethodFromMenu(int index)
    {
        if (index == 0)
        {
            forceManualInputSelection = false;
        }
        else
        {
            forceManualInputSelection = true;
            manualInputChoice = index - 1;
        }

        Debug.Log($"Input method manually set to index: {index}");
    }
}