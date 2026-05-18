using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SFXPrompt : MonoBehaviour
{
    [SerializeField]
    private string clipName;

    SFXManager sfxManager;

    private bool hasPlayed = false;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Played SFX: " + clipName);
        if (other.CompareTag("Player") && sfxManager != null && !hasPlayed)
        {
            sfxManager.PlayVoicelineSFX(clipName);
            hasPlayed = true;
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
