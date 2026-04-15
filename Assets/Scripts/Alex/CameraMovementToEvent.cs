using System;
using UnityEngine.Events;
using UnityEngine;

public class CameraMovementToEvent : MonoBehaviour
{
    [System.Serializable] public class MyEvent : UnityEvent { };
    [SerializeField] private MyEvent onFinish;

    [SerializeField] private float timeInSeconds;
    //[SerializeField] private Transform start;
    [SerializeField] private Transform finish;
    [SerializeField] private Camera cam;
    
    private float timeElapsed = 0;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = cam.transform.position;
        startRotation = cam.transform.rotation;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        float t = timeElapsed / timeInSeconds;
        cam.transform.rotation = Quaternion.Lerp(startRotation, finish.rotation, t);
        cam.transform.position = Vector3.Lerp(startPosition, finish.position, t);
        if (t >= 1)
        {
            onFinish.Invoke();
            Destroy(gameObject);
        }
    }
}
