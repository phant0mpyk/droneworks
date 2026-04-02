using UnityEngine;
using UnityEngine.InputSystem;

public class DroneInputManager : MonoBehaviour
{
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

    bool keyboardInputActive = false;
    [Header("Controller")]
    [SerializeField]
    InputActionReference leftStickInputAxis;

    [SerializeField]
    InputActionReference rightStickInputAxis;

    bool controllerInputActive = false; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        //enable the input actions, because two of them at the same time cancelled eachother
        flyWASD.action.Enable();
        flyArrows.action.Enable();
        leftStickInputAxis.action.Enable();
        rightStickInputAxis.action.Enable();
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
        }else if (keyboardInputActive)
        {
            Vector2 flyWASDInput = flyWASD.action.ReadValue<Vector2>();
            throttleAxis = flyWASDInput.y; 
            yawAxis = flyWASDInput.x;     
            Vector2 arrowInput = flyArrows.action.ReadValue<Vector2>();
            rollAxis = arrowInput.x;
            pitchAxis = arrowInput.y;
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
}
