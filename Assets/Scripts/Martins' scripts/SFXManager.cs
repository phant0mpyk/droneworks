using UnityEngine;
using System.Collections;
using TMPro;

public class SFXManager : MonoBehaviour
{
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
    public AudioSource currAudioSource;

    [SerializeField]
    private AudioSource cameraAudioSource;

    private bool playedTimedClip = false;

    [SerializeField]
    private float timeToPlayConfusedClipSeconds;
    private float currTimeToPlayConfusedClip = 0f;

    [SerializeField]
    private TextMeshProUGUI subtitles;
    bool SFXisPlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioTriggers = GetComponentsInChildren<SFXPrompt>();
        foreach (SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetManager(this);
        }
        subtitles.text = "";
    }

    void Update()
    {
        if (flightController != null)
        {
            if (flightController.GetDroneDestroyed() && GameManagerScript.gameStarted)
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

    public void PlayVoicelineEntry(AudioSFXScriptableObject _entry)
    {
        PlayVoicelineSFX(_entry, _entry.audioClipEntry.clipName, Camera.main.GetComponent<AudioSource>());
        Debug.Log("Played voiceline entry");
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
        if (currClipName == "End")
        {
            //do stuff after the end clip plays
            //insert game logic to end the game after it is played
            return;
        }
        if (_audioSource != null)
        {
            StartCoroutine(PlaySFX(_entry.audioClipEntry.audioClip, _audioSource, _entry.audioClipEntry.dutchSubtitles));
        }
        else
        {
            StartCoroutine(PlaySFX(_entry.audioClipEntry.audioClip, cameraAudioSource, _entry.audioClipEntry.dutchSubtitles));
        }
        if (_entry.audioClipEntry.audioTimedClip != null)
        {
            if (_entry.audioClipEntry.timedClipName == "PeopleWarning" || _audioSource == null)
            {
                currTimedSFXCoroutine = PlayTimedSFX(_entry.audioClipEntry.audioTimedClip, cameraAudioSource, _entry.audioClipEntry.timeToPlayTimedClipSeconds, _entry.audioClipEntry.timedDutchSubtitles);
            }
            else
            {
                currTimedSFXCoroutine = PlayTimedSFX(_entry.audioClipEntry.audioTimedClip, _audioSource, _entry.audioClipEntry.timeToPlayTimedClipSeconds, _entry.audioClipEntry.timedDutchSubtitles);
            }
        }
    }

    IEnumerator PlayTimedSFX(AudioClip timedClip, AudioSource _source, float timeToPlaySeconds, string sentence)
    {
        yield return new WaitForSeconds(timeToPlaySeconds);
        yield return new WaitWhile(() => _source.isPlaying || SFXisPlaying);
        currAudioSource = _source;
        _source.PlayOneShot(timedClip);
        SFXisPlaying = true;
        StartCoroutine(DisplaySubtitles(sentence, _source));
    }

    IEnumerator PlaySFX(AudioClip clip, AudioSource _source, string sentence)
    {
        yield return new WaitWhile(() => _source.isPlaying || SFXisPlaying);
        currAudioSource = _source;
        _source?.PlayOneShot(clip);
        SFXisPlaying = true;
        StartCoroutine(DisplaySubtitles(sentence, _source));
    }

    IEnumerator WaitForRespawn()
    {
        yield return new WaitForSeconds(4f);
        hasPlayedDestroyedSFX = false;
    }


    IEnumerator DisplaySubtitles(string sentence, AudioSource _source)
    {
        subtitles.text = sentence;
        yield return new WaitWhile(() => _source.isPlaying);
        subtitles.text = "";
        // yield return new WaitForSeconds(0.5f);
        SFXisPlaying = false;
    }

    public void RestartAllSFX()
    {
        foreach (SFXPrompt audioTrigger in audioTriggers)
        {
            audioTrigger.SetPlayed(false);
        }
    }
    public void StopCoroutines(){
        if (currTimedSFXCoroutine != null)
        {
            StopCoroutine(currTimedSFXCoroutine);
        }
    }
    public void StopAudioSource(){
        currAudioSource.Stop();
    }
}

