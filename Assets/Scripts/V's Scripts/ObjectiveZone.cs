using UnityEngine;

public class ObjectiveZone : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Which step number in the manager matches this location? (0, 1, 2, etc.)")]
    public int objectiveIndexMatch;

    [Tooltip("Does the drone need to sit inside this zone to 'Search' it?")]
    public bool requiresSearchDuration = false;
    public float searchDurationSeconds = 3f;

    private float currentSearchTimer = 0f;
    private ObjectiveManager manager;

    void Start()
    {
        manager = Object.FindFirstObjectByType<ObjectiveManager>();
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (manager == null || manager.GetCurrentObjectiveIndex() != objectiveIndexMatch) return;

        if (other.CompareTag("Player"))
        {
            if (requiresSearchDuration)
            {
                currentSearchTimer += Time.deltaTime;

                Debug.Log($"Searching... {Mathf.Round(currentSearchTimer)} / {searchDurationSeconds}");

                if (currentSearchTimer >= searchDurationSeconds)
                {
                    manager.CompleteCurrentObjective();
                }
            }
            else
            {
                manager.CompleteCurrentObjective();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentSearchTimer = 0f;
        }
    }
}