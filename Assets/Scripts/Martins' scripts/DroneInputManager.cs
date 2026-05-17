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

    // The missing serialized fields that will now show up in your Inspector:
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

    bool keyboardInputActive = false;
    bool controllerInputActive = false;

    void Start()
    {
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
    }

    void Update()
    {
        DecideInputMethod();
        ReadInput();
    }

    void DecideInputMethod()
    {
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
        // Read directly from the 4 standalone float references
        throttleAxis = throttleActionRef.action.ReadValue<float>();
        yawAxis = yawActionRef.action.ReadValue<float>();
        pitchAxis = pitchActionRef.action.ReadValue<float>();
        rollAxis = rollActionRef.action.ReadValue<float>();

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
        if (rotateControllerGimbalUp.action.IsPressed() || rotateKeyboardGimbalUp.action.IsPressed())
        {
            Debug.Log("UP");
            flightController.RotateGimbalUp();
        }
        else if (rotateControllerGimbalDown.action.IsPressed() || rotateKeyboardGimbalDown.action.IsPressed())
        {
            Debug.Log("Down");
            flightController.RotateGimbalDown();
        }
    }
}