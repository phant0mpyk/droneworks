using Unity.VisualScripting;
using UnityEngine;

public class HighlightTrigger : MonoBehaviour
{
    [SerializeField] Material highlightMaterial;
    [SerializeField] private bool turnOn;
    [SerializeField] GameObject[] objectsToHighlight;
    [SerializeField] private bool customTimer;
    [SerializeField] private float timer;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (var obj in objectsToHighlight)
            {
                ToggleHighlight toggleScript;
                if (!obj.TryGetComponent(out toggleScript))
                {
                    obj.AddComponent<ToggleHighlight>().highlightMaterial = highlightMaterial;
                    toggleScript = obj.GetComponent<ToggleHighlight>();
                }

                if (customTimer)
                {
                    toggleScript.timer = timer;
                    toggleScript.Toggle(turnOn,timer);
                }
                else
                {
                    toggleScript.Toggle(turnOn,999999);
                }
            }
            

        }
    }
}
