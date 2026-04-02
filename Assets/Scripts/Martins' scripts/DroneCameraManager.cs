using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    [Space(1)]
    [SerializeField]
    Camera droneFrontCamera;

    [Tooltip("Camera up-tilt in degrees. In FPV you need to see in front and above the drone, because these ones tend to be faster. 0-10 is standard for cinematic drones, 10-25 for FPV freestyle drones, above 25 for racing drones.")]
    [SerializeField]
    float droneAcrobaticFrontCameraTilt = 0f;

    [Tooltip("Camera down-tilt in degrees. Usually with more cinematic drones the camera points down to see landscape.")]
    [SerializeField]
    float droneStabilizedFrontCameraTilt = 0f;

    [SerializeField]
    Camera droneBottomCamera;  

    public enum CameraPosition { Front, Bottom }
    [SerializeField]
    CameraPosition currCameraPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FlightController droneScript = GetComponentInParent<FlightController>();
        SetCamera(droneScript.GetCurrFlightMode());
        droneFrontCamera?.gameObject.SetActive(true);
        droneBottomCamera?.gameObject.SetActive(false);
        ToggleCameraPosition(currCameraPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCamera(FlightController.FlightMode _flightMode)
    {
        switch (_flightMode)
        {
            case FlightController.FlightMode.Acrobatic:
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                //tilt of the camera
                droneFrontCamera.transform.localRotation = Quaternion.Euler(-droneAcrobaticFrontCameraTilt, 0f, 0f);
                break;
            case FlightController.FlightMode.Stabilized:
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                //tilt of the camera
                droneFrontCamera.transform.localRotation = Quaternion.Euler(droneStabilizedFrontCameraTilt, 0f, 0f);
                break;
            default:
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                break;
        }   
    }

    public void ToggleCameraPosition(CameraPosition _cameraPosition)
    {
        switch (_cameraPosition)
        {
            case CameraPosition.Front:
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                break;
            case CameraPosition.Bottom:
                droneFrontCamera?.gameObject.SetActive(false);
                droneBottomCamera?.gameObject.SetActive(true);
                break;
            default:
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                break;
        }
    }
}
