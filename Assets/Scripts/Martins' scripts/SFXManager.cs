using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    [SerializeField]
    AudioSFXScriptableObject aliveEntry;
    [SerializeField]
    AudioSFXScriptableObject deathEntry;

    [SerializeField]
    AudioSFXScriptableObject destroyedEntry;

    private bool hasPlayedDestroyedSFX = false;

    [SerializeField]
    FlightController flightController;

    [SerializeField]
    private SFXPrompt[] audioTriggers;

    string currClipName;
    IEnumerator currSFXCoroutine;
    IEnumerator currTimedSFXCoroutine;

    [SerializeField]
    private AudioSource cameraAudioSource;

    private bool playedTimedClip = false;

    [SerializeField]
    private float timeToPlayConfusedClipSeconds;
    private float currTimeToPlayConfusedClip = 0f;

    [SerializeField]
    private float timeToLetHimDieMinutes;
    private float currTimeToLetHimDieSeconds = 0f;
    private bool timeToLetHimDie = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioTriggers = GetComponentsInChildren<SFXPrompt>();
        foreach(SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetManager(this);
        }
    }

    void Update()
    {
        currTimeToLetHimDieSeconds += Time.deltaTime;
        if(currTimeToLetHimDieSeconds >= timeToLetHimDieMinutes * 60f && !timeToLetHimDie)
        {
            timeToLetHimDie = true;
        }
        if(flightController != null)
        {
            if (flightController.GetDroneDestroyed())
            {
                if (!hasPlayedDestroyedSFX)
                {
                    PlayVoicelineSFX(destroyedEntry, destroyedEntry.audioClipEntry.clipName, null);
                    hasPlayedDestroyedSFX = true;
                    StartCoroutine(WaitForRespawn());
                }
            }
        }
    }

    public void PlayVoicelineSFX(AudioSFXScriptableObject _entry, string _entryName, AudioSource _audioSource)
    {
        currTimeToPlayConfusedClip = 0f;
        if (currSFXCoroutine != null)
        {
            StopCoroutine(currSFXCoroutine);
        }
        if(currTimedSFXCoroutine != null)
        {
            StopCoroutine(currTimedSFXCoroutine);
        }
        if(cameraAudioSource.isPlaying)
        {
            cameraAudioSource.Stop();
        }
        currClipName = _entryName;
        if(currClipName == "End")
        {
            //do stuff after the end clip plays
            //insert game logic to end the game after it is played
            return;
        }
        if(_audioSource != null)
        {
            _audioSource?.PlayOneShot(_entry.audioClipEntry.audioClip);
        }
        else
        {
            cameraAudioSource?.PlayOneShot(_entry.audioClipEntry.audioClip);
        }
        if (_entry.audioClipEntry.audioTimedClip != null)
        {
            currTimedSFXCoroutine = PlayTimedSFX(_entry.audioClipEntry.audioTimedClip, _entry.audioClipEntry.timeToPlayTimedClipSeconds);
            StartCoroutine(currTimedSFXCoroutine);
        }   
    }

    IEnumerator PlayTimedSFX(AudioClip timedClip, float timeToPlaySeconds)
    {
        yield return new WaitForSeconds(timeToPlaySeconds);
        cameraAudioSource?.PlayOneShot(timedClip);
    }

    IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(4f);
        hasPlayedDestroyedSFX = false;
    }

    public void RestartAllSFX()
    {
        foreach(SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetPlayed(false);
        }
    }
}
