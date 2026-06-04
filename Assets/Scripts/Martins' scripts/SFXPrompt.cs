using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SFXPrompt : MonoBehaviour
{
    [SerializeField]
    private string entryName;
    [SerializeField]
    AudioSFXScriptableObject entryToPlay;
    
    SFXManager sfxManager;

    private bool hasPlayed = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && sfxManager != null && (!hasPlayed || entryToPlay.audioClipEntry.repeatable))
        {
            if(TryGetComponent<AudioSource>(out AudioSource audioSource))
            {
                sfxManager?.PlayVoicelineSFX(entryToPlay, entryName, audioSource);
                hasPlayed = true;
            }
            else
            {
                sfxManager?.PlayVoicelineSFX(entryToPlay, entryName, null);
                hasPlayed = true; 
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
