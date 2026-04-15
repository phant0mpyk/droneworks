using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MainMenuLoader : MonoBehaviour
{
    [Tooltip("mainmenu name")]
    public string mainMenuSceneName = "MainMenu";

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}