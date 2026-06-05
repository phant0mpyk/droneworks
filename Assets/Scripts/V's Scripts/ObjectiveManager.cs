using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    [Header("UI Link")]
    [SerializeField] private TextMeshProUGUI objectiveTextUI;

    private string[] objectives = new string[]
    {
        "Find burning cabin",
        "Get inside of the cabin",
        "Search the cabin",
        "Continue searching along path",
        "Explore the Lake or the Cave?",
        "Search in the chosen area",
        "Found missing person!"
    };

    private int currentObjectiveIndex = 0;

    void Start()
    {
        UpdateObjectiveUI();
    }

    public int GetCurrentObjectiveIndex() => currentObjectiveIndex;

    public void CompleteCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length - 1) return;

        currentObjectiveIndex++;
        UpdateObjectiveUI();

        Debug.Log($"Objective Updated to: {objectives[currentObjectiveIndex]}");
    }

    public void OverrideObjectiveText(string alternativeText)
    {
        if (objectiveTextUI != null)
        {
            objectiveTextUI.text =  alternativeText;
        }
    }

    void UpdateObjectiveUI()
    {
        if (objectiveTextUI != null)
        {
            objectiveTextUI.text = objectives[currentObjectiveIndex];
        }
    }
}