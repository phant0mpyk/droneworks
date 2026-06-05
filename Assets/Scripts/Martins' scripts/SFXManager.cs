using UnityEngine;
using System.Collections;
using TMPro;

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

    [SerializeField]
    private TextMeshProUGUI subtitles;
    bool SFXisPlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioTriggers = GetComponentsInChildren<SFXPrompt>();
        foreach(SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetManager(this);
        }
        subtitles.text = "";
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
        /*if (currSFXCoroutine != null)
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
        }*/
        currClipName = _entryName;
        if(currClipName == "End")
        {
            //do stuff after the end clip plays
            //insert game logic to end the game after it is played
            return;
        }
        if(_audioSource != null)
        {
            StartCoroutine(PlaySFX(_entry.audioClipEntry.audioClip, _audioSource, _entry.audioClipEntry.dutchSubtitles));

        }
        else
        {
            StartCoroutine(PlaySFX(_entry.audioClipEntry.audioClip, cameraAudioSource, _entry.audioClipEntry.dutchSubtitles));
        }
        if (_entry.audioClipEntry.audioTimedClip != null)
        {
            currTimedSFXCoroutine = PlayTimedSFX(_entry.audioClipEntry.audioTimedClip, _entry.audioClipEntry.timeToPlayTimedClipSeconds, _entry.audioClipEntry.timedDutchSubtitles);
            StartCoroutine(currTimedSFXCoroutine);
        }   
    }

    IEnumerator PlayTimedSFX(AudioClip timedClip, float timeToPlaySeconds, string sentence)
    {
        yield return new WaitForSeconds(timeToPlaySeconds);
        yield return new WaitWhile(() => cameraAudioSource.isPlaying || SFXisPlaying);
        cameraAudioSource?.PlayOneShot(timedClip);
        SFXisPlaying = true;
        StartCoroutine(DisplaySubtitles(sentence, cameraAudioSource, timedClip));
    }

    IEnumerator PlaySFX(AudioClip clip, AudioSource _source, string sentence)
    {
        yield return new WaitWhile(() => _source.isPlaying || SFXisPlaying);
        _source?.PlayOneShot(clip);
        SFXisPlaying = true;
        StartCoroutine(DisplaySubtitles(sentence, cameraAudioSource, clip));
    }

    IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(4f);
        hasPlayedDestroyedSFX = false;
    }


    IEnumerator DisplaySubtitles(string sentence, AudioSource _source, AudioClip _clip)
    {
        subtitles.text = sentence;
        yield return new WaitWhile(() => _source.isPlaying);
        if (_source.clip == _clip)    
        {
            subtitles.text = "";
        }
        yield return new WaitForSeconds(2f);
        SFXisPlaying = false;
    }

    public void RestartAllSFX()
    {
        foreach(SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetPlayed(false);
        }
    }
}
