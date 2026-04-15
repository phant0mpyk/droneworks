using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second on each axis")]
    public Vector3 rotationSpeed = new Vector3(0, 50, 0);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}