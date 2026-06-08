using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";
    public string tutorialSceneName = "TutorialScene";

    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;

    private void Start()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void ToggleCredits(bool open)
    {
        if (mainPanel != null && creditsPanel != null)
        {
            //mainPanel.SetActive(!open);
            creditsPanel.SetActive(open);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Player has exited the game.");
        Application.Quit();
    }
}