using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class DiegeticMenu : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineCamera mainCam;
    public CinemachineCamera mapCam;
    public CinemachineCamera settingsCam;

    [Header("Scene Settings")]
    public string gameSceneName = "DroneUITest";
    public void GoToMap() => SwitchCamera(mapCam);
    public void GoToSettings() => SwitchCamera(settingsCam);
    public void GoToMain() => SwitchCamera(mainCam);
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void SwitchCamera(CinemachineCamera targetCam)
    {
        mainCam.Priority = 10;
        mapCam.Priority = 10;
        settingsCam.Priority = 10;
        targetCam.Priority = 20;
    }
}