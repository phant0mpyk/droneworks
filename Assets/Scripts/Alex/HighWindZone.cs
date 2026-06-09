using UnityEngine;

public class HighWindZone : MonoBehaviour
{

    [SerializeField] private float windSpeed;
    float defaultWindSpeed;
    private Wind wind;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            wind = other.transform.Find("Wind").GetComponent<Wind>();
            defaultWindSpeed = wind.windSpeed;
            wind.windSpeed = windSpeed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            wind.windSpeed = defaultWindSpeed;
        }
    }
}
