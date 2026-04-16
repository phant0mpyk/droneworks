using UnityEngine;

public class ObjectivePicker : MonoBehaviour
{
    [SerializeField] private GameObject[] objectives;
    void Start()
    {
        foreach (GameObject objective in objectives)
        {
            objective.SetActive(false);
        }
        int winner = Random.Range(0, objectives.Length);
        objectives[winner].SetActive(true);
    }

}
