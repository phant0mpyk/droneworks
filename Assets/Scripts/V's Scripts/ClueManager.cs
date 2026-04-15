using UnityEngine;
using TMPro;

public sealed class ClueManager : MonoBehaviour
{
    public int totalClues = 3;
    private int _cluesFound = 0;

    [Header("UI References")]
    public TextMeshProUGUI counterText;
    public GameObject popupText;

    void Start()
    {
        UpdateUI();
        if (popupText != null) popupText.SetActive(false);
    }

    public void IncrementClues()
    {
        _cluesFound++;
        UpdateUI();
        TriggerPopup();
    }

    private void UpdateUI()
    {
        counterText.text = $"{_cluesFound}/{totalClues}";
    }

    private void TriggerPopup()
    {
        if (popupText != null)
        {
            popupText.SetActive(true);
            CancelInvoke(nameof(HidePopup));
            Invoke(nameof(HidePopup), 2f);
        }
    }

    private void HidePopup() => popupText.SetActive(false);
}