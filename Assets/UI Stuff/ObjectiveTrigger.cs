using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objective;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<DroneUIManager>().currentObjective = objective;
        }
    }
}
