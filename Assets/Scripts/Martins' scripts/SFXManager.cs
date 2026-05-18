using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    [SerializeField]
    private AudioSFXScriptableObject audioSFXData;

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
        currTimeToPlayConfusedClip += Time.deltaTime;
        currTimeToLetHimDieSeconds += Time.deltaTime;
        if(currTimeToPlayConfusedClip >= timeToPlayConfusedClipSeconds)
        {
            PlayVoicelineSFX("TakingTooLong");
            currTimeToPlayConfusedClip = 0f;
        }
        if(currTimeToLetHimDieSeconds >= timeToLetHimDieMinutes * 60f && !timeToLetHimDie)
        {
            timeToLetHimDie = true;
        }
    }

    public void PlayVoicelineSFX(string clipName)
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
        currClipName = clipName;
        foreach (AudioClipEntry entry in audioSFXData.audioClipEntries)
        {
            if (entry.clipName == currClipName)
            {
                if(currClipName == "End")
                {
                    if(!timeToLetHimDie)
                    {
                        cameraAudioSource.PlayOneShot(entry.audioClip);
                    }
                    else
                    {
                        cameraAudioSource.PlayOneShot(entry.audioTimedClip);
                    }
                    return;
                }
                cameraAudioSource.PlayOneShot(entry.audioClip);
                if (entry.audioTimedClip != null)
                {
                    currTimedSFXCoroutine = PlayTimedSFX(entry.audioTimedClip, entry.timeToPlayTimedClipSeconds);
                    StartCoroutine(currTimedSFXCoroutine);
                }   
            }
        }
    }

    IEnumerator PlayTimedSFX(AudioClip timedClip, float timeToPlaySeconds)
    {
        yield return new WaitForSeconds(timeToPlaySeconds);
        cameraAudioSource.PlayOneShot(timedClip);
    }

    public void RestartAllSFX()
    {
        foreach(SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetPlayed(false);
        }
    }
}
