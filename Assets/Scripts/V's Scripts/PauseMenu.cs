using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private TMP_Dropdown inputDropdown;

    [Header("Input Manager Link")]
    [SerializeField] private DroneInputManager droneInputManager;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);

        if (inputDropdown != null && droneInputManager != null)
        {
            inputDropdown.onValueChanged.AddListener(droneInputManager.SetInputMethodFromMenu);
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
}