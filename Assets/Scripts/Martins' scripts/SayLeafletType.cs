using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SayLeafletType : MonoBehaviour
{
    [SerializeField]
    private string entryName;
    [SerializeField]
    AudioSFXScriptableObject entryWhenCave;
    [SerializeField]
    AudioSFXScriptableObject entryWhenLake;
    
    SFXManager sfxManager;

    private bool hasPlayed = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && sfxManager != null && (!hasPlayed))
        {
            if(TryGetComponent<AudioSource>(out AudioSource audioSource))
            {
                switch (GameManagerScript.instance.victimSpawnLocation)
                {
                    case GameManagerScript.VictimSpawn.Lake1:
                        sfxManager?.PlayVoicelineSFX(entryWhenLake, entryName, audioSource);
                        hasPlayed = true;
                        break;
                    case GameManagerScript.VictimSpawn.Lake2:
                        sfxManager?.PlayVoicelineSFX(entryWhenLake, entryName, audioSource);
                        hasPlayed = true;
                        break;
                    case GameManagerScript.VictimSpawn.Cave1:
                        sfxManager?.PlayVoicelineSFX(entryWhenCave, entryName, audioSource);
                        hasPlayed = true;
                        break;
                    case GameManagerScript.VictimSpawn.Cave2:
                        sfxManager?.PlayVoicelineSFX(entryWhenCave, entryName, audioSource);
                        hasPlayed = true;
                        break;
                }
            }
        }
    }

    public void SetManager(SFXManager manager)
    {
        sfxManager = manager;
    }

    public void SetPlayed(bool played)
    {
        hasPlayed = played;
    }
}
