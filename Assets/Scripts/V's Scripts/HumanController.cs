using UnityEngine;
using UnityEngine.InputSystem;

public class HumanController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.15f;

    [Header("Cameras")]
    public Camera humanCamera;
    public Camera droneCamera;

    private float verticalRotation = 0f;

    void Start()
    {
        if (Mouse.current != null)
        {
            InputSystem.EnableDevice(Mouse.current);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            if (delta.magnitude > 0)
            {
                Debug.Log("Mouse moving: " + delta);
            }
        }

        if (Keyboard.current == null || Mouse.current == null) return;

        float x = 0;
        float z = 0;

        if (Keyboard.current.wKey.isPressed) z += 1;
        if (Keyboard.current.sKey.isPressed) z -= 1;
        if (Keyboard.current.aKey.isPressed) x -= 1;
        if (Keyboard.current.dKey.isPressed) x += 1;

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (humanCamera != null && humanCamera.enabled)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);

            verticalRotation -= mouseDelta.y * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            humanCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            TogglePOV();
        }
    }

    void TogglePOV()
    {
        if (humanCamera == null || droneCamera == null) return;

        bool showHuman = !humanCamera.enabled;
        humanCamera.enabled = showHuman;
        droneCamera.enabled = !showHuman;

        Cursor.lockState = showHuman ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !showHuman;
    }
}