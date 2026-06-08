using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class SequentialSFXPrompt : MonoBehaviour
{
    [System.Serializable]
    public class VoicelineStep
    {
        public string entryName;
        public AudioSFXScriptableObject entryToPlay;
        public float durationToWait = 3.0f;
    }

    [Header("Sequential Voicelines Setup")]
    [SerializeField] private List<VoicelineStep> voicelineSequence;

    private SFXManager sfxManager;
    private bool hasPlayed = false;

    // ADDED START FUNCTION TO FIND THE MANAGER AUTOMATICALLY
    private void Start()
    {
        if (sfxManager == null)
        {
            sfxManager = Object.FindFirstObjectByType<SFXManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && sfxManager != null && !hasPlayed)
        {
            hasPlayed = true;
            StartCoroutine(PlayVoicelineSequence());
        }
    }

    private IEnumerator PlayVoicelineSequence()
    {
        TryGetComponent<AudioSource>(out AudioSource audioSource);

        foreach (VoicelineStep step in voicelineSequence)
        {
            if (step.entryToPlay == null) continue;

            sfxManager.PlayVoicelineSFX(step.entryToPlay, step.entryName, audioSource);

            yield return new WaitForSeconds(step.durationToWait);
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