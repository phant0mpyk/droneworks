using UnityEngine;

[System.Serializable]
public class AudioClipEntry
{
    public string clipName;
    public AudioClip audioClip;
    public float timeToPlayTimedClipSeconds;
    public AudioClip audioTimedClip;
}

[CreateAssetMenu(fileName = "AudioSFXScriptableObject", menuName = "Scriptable Objects/AudioSFXScriptableObject")]
public class AudioSFXScriptableObject : ScriptableObject
{
    public AudioClipEntry[] audioClipEntries;
}
