using System;
using UnityEngine;

public class DroneCameraManager : MonoBehaviour
{
    [Header("Cameras")]
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
    Vector2 cameraGimbalRotationCap;
    CameraPosition currCameraPosition = CameraPosition.Front;
    public enum CameraGimbalRotationDirection {Up = 1, Down = -1, None = 0};
    CameraGimbalRotationDirection currCameraGimbalRotationDirection = CameraGimbalRotationDirection.None;

    float currGimbalAngle;

    float currGimbalAngleFromWorldUp;

    [SerializeField]
    float cameraGimbalRotationSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FlightController droneScript = GetComponentInParent<FlightController>();
        switch (droneScript.GetCurrFlightMode())
        {
            case FlightController.FlightMode.Acrobatic:
                SetCamera(FlightController.FlightMode.Acrobatic);
                currGimbalAngle = droneAcrobaticFrontCameraTilt;
                break;
            case FlightController.FlightMode.Stabilized:
                SetCamera(FlightController.FlightMode.Stabilized);
                currGimbalAngle = -droneStabilizedFrontCameraTilt;
                break;
        }
        droneFrontCamera?.gameObject.SetActive(true);
        droneBottomCamera?.gameObject.SetActive(false);
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

    public void ToggleCameraPosition()
    {
        switch (currCameraPosition)
        {
            case CameraPosition.Front:
                currCameraPosition = CameraPosition.Bottom;
                droneFrontCamera?.gameObject.SetActive(false);
                droneBottomCamera?.gameObject.SetActive(true);
                break;
            case CameraPosition.Bottom:
                currCameraPosition = CameraPosition.Front;
                droneFrontCamera?.gameObject.SetActive(true);
                droneBottomCamera?.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void RotateCameraGimbal(CameraGimbalRotationDirection _rotationDirection)
    {
        currCameraGimbalRotationDirection = _rotationDirection;
        currGimbalAngle += (int)currCameraGimbalRotationDirection * cameraGimbalRotationSpeed * Time.deltaTime;
        currGimbalAngle = Mathf.Clamp(currGimbalAngle, cameraGimbalRotationCap.x,cameraGimbalRotationCap.y);
    }

    public void AdjustGimbalAngle(Vector3 worldVectorUp, Vector3 droneVectorForward)
    {
        Transform drone = transform.parent;
        //forward vector of the drone but flattened horizontally without pitch or roll
        Vector3 droneYawOnlyForward = Vector3.ProjectOnPlane(drone.forward, worldVectorUp).normalized;
        //adjustment so the camera doesn't glitch out when it's close to the rotation point
        if (droneYawOnlyForward.sqrMagnitude < 0.001f) return;
        //drone camera looks only along horizon line 
        Quaternion droneYaw = Quaternion.LookRotation(droneYawOnlyForward, worldVectorUp);
        //adds the rotation of camera up/down that can be adjusted by the player
        Quaternion gimbal = Quaternion.Euler(-currGimbalAngle, 0f, 0f);
        droneFrontCamera.transform.rotation = droneYaw * gimbal;
    }
}
