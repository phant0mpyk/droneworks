using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance {get; private set;}
    [SerializeField] 
    private GameObject introductionCutscene;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject[] playerCanvases;

    public enum VictimSpawn{Lake1, Lake2, Cave1, Cave2};
    [Header("Victim Settings")]
    bool victimFound = false;
    [SerializeField]
    float victimFoundTimerSeconds;
    float currVictimFoundTimerSeconds = 0f;
    public VictimSpawn victimSpawnLocation { get; private set; }
    [SerializeField]
    public GameObject[] victimGameObjectSpawns;
    [SerializeField] 
    private GameObject[] victimNearbyPrompts;
    [SerializeField]
    private GameObject[] victimLakeItems;
    [SerializeField]
    private GameObject[] victimCaveItems;
    public enum Language { English, Dutch }
    [Header("Language Settings")]
    public static Language currentLanguage { get; set; }
    [SerializeField]
    public GameObject[] englishObjects;
    [SerializeField]
    public GameObject[] dutchObjects;

    public static bool gameStarted { get; private set; }
    void Awake()
    {
        gameStarted = false;
        currentLanguage = Language.Dutch;
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnTranslatedObjects();
        SpawnVictim();
        SpawnVictimItems();
        introductionCutscene.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(victimFound)
        {
            currVictimFoundTimerSeconds += Time.deltaTime;
            if(currVictimFoundTimerSeconds >= victimFoundTimerSeconds){
                GameEnd();
            }
        }
    }

    public void IntroductionFinished()
    {
        introductionCutscene.SetActive(false);
        player.SetActive(true);
        foreach (var canvas in playerCanvases)
        {
            canvas.SetActive(true);
        }
        gameStarted = true;
    }
    private void SpawnVictim()
    {
        int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(VictimSpawn)).Length);
        victimSpawnLocation = (VictimSpawn)randomIndex;
        for(int i = 0; i < victimGameObjectSpawns.Length; i++)
        {
            if(i == randomIndex)
            {
                victimGameObjectSpawns[i].SetActive(true);
                victimNearbyPrompts[i].SetActive(true);
            }
            else
            {
                victimGameObjectSpawns[i].SetActive(false);
                victimNearbyPrompts[i].SetActive(false);
            }
        }
    }

    private void SpawnVictimItems()
    {
        switch (victimSpawnLocation)
        {
            case VictimSpawn.Lake1:
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(true);
                }
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(false);
                }
                victimGameObjectSpawns[(int)VictimSpawn.Lake1].SetActive(true);
                break;
            case VictimSpawn.Lake2:
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(true);
                }
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(false);
                }
                victimGameObjectSpawns[(int)VictimSpawn.Lake2].SetActive(true);
                break;
            case VictimSpawn.Cave1:
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(true);
                }
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(false);
                }
                victimGameObjectSpawns[(int)VictimSpawn.Cave1].SetActive(true);
                break;
            case VictimSpawn.Cave2:
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(true);
                }
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(false);
                }
                victimGameObjectSpawns[(int)VictimSpawn.Cave2].SetActive(true);
                break;
        }
    }
    private void SpawnTranslatedObjects()
    {
        switch (currentLanguage)
        {
            case Language.English:
                for(int i = 0; i < englishObjects.Length; i++)
                {
                    englishObjects[i].SetActive(true);
                }
                for(int i = 0; i < dutchObjects.Length; i++)
                {
                    dutchObjects[i].SetActive(false);
                }
                break;
            case Language.Dutch:
                for(int i = 0; i < dutchObjects.Length; i++)
                {
                    dutchObjects[i].SetActive(true);
                }
                for(int i = 0; i < englishObjects.Length; i++)
                {
                    englishObjects[i].SetActive(false);
                }
                break;
        }
    }
    public void SetVictimFound(bool found)
    {
        victimFound = found;
    }
    //V, put stuff with UI and other logic here so it starts when the game ends and the victim is found
    private void GameEnd()
    {
        
    }
}
