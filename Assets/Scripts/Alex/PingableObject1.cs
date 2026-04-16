using System;
using UnityEngine;
using UnityEngine.Events;

public class PingableObject1 : MonoBehaviour
{
    [SerializeField] private float radius;
    private SphereCollider sphereCollider;
    
    [System.Serializable] public class MyEvent : UnityEvent {}

    [SerializeField] private MyEvent onPing;
    [SerializeField] private Transform trans;
    
    void Start()
    {
        if (!TryGetComponent(out sphereCollider))
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.radius = radius;
        sphereCollider.isTrigger = true;
        sphereCollider.center = trans.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ping"))
        {
            onPing.Invoke();
        }
    }
}
