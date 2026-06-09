using Unity.VisualScripting;
using UnityEngine;

public class HighlightTrigger : MonoBehaviour
{
    [SerializeField] Material highlightMaterial;
    [SerializeField] private bool turnOn;
    [SerializeField] GameObject objectToHighlight;
    [SerializeField] private bool customTimer;
    [SerializeField] private float timer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ToggleHighlight toggleScript;
            if (!objectToHighlight.TryGetComponent(out toggleScript))
            {
                objectToHighlight.AddComponent<ToggleHighlight>().highlightMaterial = highlightMaterial;
                toggleScript = objectToHighlight.GetComponent<ToggleHighlight>();
            }

            if (customTimer)
            {
                toggleScript.timer = timer;
                //toggleScript.Toggle(turnOn,timer);
            }
            else
            {
                //toggleScript.Toggle(turnOn,999999);
            }

        }
    }
}
