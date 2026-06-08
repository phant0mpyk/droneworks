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

    public enum VictimSpawn{Lake, Cave};
    [Header("Victim Spawn Settings")]
    public static VictimSpawn victimSpawnLocation { get; private set; }
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
        ChooseVictimSpawn();
        SpawnVictimItems();
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
        }
        gameStarted = true;
    }
    private void ChooseVictimSpawn()
    {
        int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(VictimSpawn)).Length);
        victimSpawnLocation = (VictimSpawn)randomIndex;
    }

    private void SpawnVictimItems()
    {
        switch (victimSpawnLocation)
        {
            case VictimSpawn.Lake:
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(true);
                }
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(false);
                }
                break;
            case VictimSpawn.Cave:
                for(int i = 0; i < victimCaveItems.Length; i++)
                {
                    victimCaveItems[i].SetActive(true);
                }
                for(int i = 0; i < victimLakeItems.Length; i++)
                {
                    victimLakeItems[i].SetActive(false);
                }
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
}
