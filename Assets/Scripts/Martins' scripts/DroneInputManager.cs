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

    [Header("Input")]
    [Header("Keyboard")]
    [SerializeField] 
    InputActionReference flyWASD;

    [SerializeField] 
    InputActionReference flyArrows;

    [SerializeField]
    InputActionReference toggleCameraKeyboard;

    [SerializeField]
    InputActionReference rotateKeyboardGimbalUp;

    [SerializeField]
    InputActionReference rotateKeyboardGimbalDown;

    [SerializeField]
    InputActionReference toggleKeyboardThermalVision;

    [SerializeField]
    InputActionReference pingKeyboard;

    bool keyboardInputActive = false;
    [Header("Controller")]
    [SerializeField]
    InputActionReference leftStickInputAxis;

    [SerializeField]
    InputActionReference rightStickInputAxis;

    [SerializeField]
    InputActionReference toggleCameraController;

    [SerializeField]
    InputActionReference rotateControllerGimbalUp;

    [SerializeField]
    InputActionReference rotateControllerGimbalDown;

    [SerializeField]
    InputActionReference toggleControllerThermalVision;

    [SerializeField]
    InputActionReference pingController;
    bool controllerInputActive = false; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {

    }

    void Start()
    {
        //enable the input actions, because two of them at the same time cancelled eachother
        flyWASD.action.Enable();
        flyArrows.action.Enable();
        leftStickInputAxis.action.Enable();
        rightStickInputAxis.action.Enable();
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
        
        toggleKeyboardThermalVision.action.performed += ToggleThermalVision;
        toggleControllerThermalVision.action.performed += ToggleThermalVision;
        toggleCameraController.action.performed += CameraToggle;
        toggleCameraKeyboard.action.performed += CameraToggle;
    }

    // Update is called once per frame
    void Update()
    {
        DecideInputMethod();
        ReadInput();
    }

    void DecideInputMethod()
    {
        if(flyWASD.action.triggered || flyArrows.action.triggered)
        {
            controllerInputActive = false;
            keyboardInputActive = true;
        }else if (leftStickInputAxis.action.triggered || rightStickInputAxis.action.triggered)
        {
            controllerInputActive = true;
            keyboardInputActive = false;
        }
    }

    void ReadInput()
    {
        //input depending on the currently active controls
        //looks like action.triggered only fires once when the input is first detected so there are booleans which indicate which layout is active instead
        if (controllerInputActive)
        {
            Vector2 flyLeftStickInput = leftStickInputAxis.action.ReadValue<Vector2>();
            Vector2 flyRightStickInput = rightStickInputAxis.action.ReadValue<Vector2>();
            throttleAxis = flyLeftStickInput.y;
            yawAxis = flyLeftStickInput.x;
            pitchAxis = flyRightStickInput.y;
            rollAxis = flyRightStickInput.x;
            //rotation only allowed for controller, because it's a feature on the dji controller
            //also it might be too much for the keyboard drone, since it's moves are very erratic, so there will only be toggle with that one
            GimbalRotation();
        }else if (keyboardInputActive)
        {
            Vector2 flyWASDInput = flyWASD.action.ReadValue<Vector2>();
            throttleAxis = flyWASDInput.y; 
            yawAxis = flyWASDInput.x;     
            Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
            rollAxis = arrowInput.x;
            pitchAxis = arrowInput.y;
            GimbalRotation();
        }
    }

    public float GetThrottleAxis()
    {
        return throttleAxis;
    }

    public float GetYawAxis()
    {
        return yawAxis;
    }

    public float GetPitchAxis()
    {
        return pitchAxis;
    }

    public float GetRollAxis()
    {
        return rollAxis;
    }

    private void CameraToggle(InputAction.CallbackContext context)
    {
        flightController.OnCameraToggle();
    }

    private void Ping(InputAction.CallbackContext context)
    {
        flightController.Ping();
    }

    private void ToggleThermalVision(InputAction.CallbackContext context)
    {
        flightController.ToggleThermalVision();
    }

    private void GimbalRotation()
    {
            if (rotateControllerGimbalUp.action.IsPressed() || rotateKeyboardGimbalUp.action.IsPressed())
            {
                Debug.Log("UP");
                flightController.RotateGimbalUp();
            }else if(rotateControllerGimbalDown.action.IsPressed() || rotateKeyboardGimbalDown.action.IsPressed())
            {
                Debug.Log("Down");
                flightController.RotateGimbalDown();
            }
    }
}
