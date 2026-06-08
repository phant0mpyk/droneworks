using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    
    [SerializeField] 
    private GameObject introductionCutscene;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject[] playerCanvases;

    public static bool GameStarted { get; private set; }
    void Awake()
    {
        GameStarted = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        introductionCutscene.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IntroductionFinished()
    {
        introductionCutscene.SetActive(false);
        player.SetActive(true);
        foreach (var canvas in playerCanvases)
        {
            canvas.SetActive(true);
            GameStarted = true;
        }
    }
}
