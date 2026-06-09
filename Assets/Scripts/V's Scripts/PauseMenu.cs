using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private TMP_Dropdown inputDropdown;
    [SerializeField] private TMP_Dropdown flightModeDropdown;

    [Header("Component Links")]
    [SerializeField] private DroneInputManager droneInputManager;
    [SerializeField] private FlightController flightController;

    [Header("Scene Settings")]
    [SerializeField] private string titleScreenSceneName = "TitleScreen";

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);

        if (inputDropdown != null && droneInputManager != null)
        {
            inputDropdown.onValueChanged.AddListener(droneInputManager.SetInputMethodFromMenu);
        }

        if (flightModeDropdown != null && flightController != null)
        {
            flightModeDropdown.onValueChanged.AddListener(SetFlightModeFromMenu);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetFlightModeFromMenu(int index)
    {
        if (flightController == null) return;
        switch (index)
        {
            case 0:
                flightController.SetFlightModeAcrobatic();
                break;
            case 1:
                flightController.SetFlightModeStabilizedThrottle();
                break;
            case 2:
                flightController.SetFlightModeStabilizedHeight();
                break;
            default:
                Debug.LogWarning($"Unknown flight mode selection index: {index}");
                break;
        }
    }

    public void ReturnToTitleScreen()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleScreenSceneName);
    }
}