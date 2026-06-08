using UnityEngine;

[System.Serializable]
public class AudioClipEntry
{
    public bool repeatable;
    public string clipName;
    public string englishSpeakerName;
    public string dutchSpeakerName;
    public string englishSubtitles;
    public string dutchSubtitles;
    public AudioClip audioClip;
    public AudioClip audioClipAlternative;
    public float timeToPlayTimedClipSeconds;
    public string timedClipName;
    public string timedEnglishSpeakerName;
    public string timedDutchSpeakerName;
    public string timedEnglishSubtitles;
    public string timedDutchSubtitles;
    public AudioClip audioTimedClip;
    public AudioClip audioTimedClipAlternative;
}

[CreateAssetMenu(fileName = "AudioSFXScriptableObject", menuName = "Scriptable Objects/AudioSFXScriptableObject")]
public class AudioSFXScriptableObject : ScriptableObject
{
    public AudioClipEntry audioClipEntry;
}
