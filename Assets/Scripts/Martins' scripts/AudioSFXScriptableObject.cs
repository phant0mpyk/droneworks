using UnityEngine;

[System.Serializable]
public class AudioClipEntry
{
    public bool repeatable;
    public string clipName;
    public string englishSubtitles;
    public string dutchSubtitles;
    public AudioClip audioClip;
    public float timeToPlayTimedClipSeconds;
    public string timedClipName;
    public string timedEnglishSubtitles;
    public string timedDutchSubtitles;
    public AudioClip audioTimedClip;
}

[CreateAssetMenu(fileName = "AudioSFXScriptableObject", menuName = "Scriptable Objects/AudioSFXScriptableObject")]
public class AudioSFXScriptableObject : ScriptableObject
{
    public AudioClipEntry audioClipEntry;
}
