using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    [SerializeField]
    private AudioSFXScriptableObject audioSFXData;

    [SerializeField]
    private GameObject[] audioSourceColliders;

    [SerializeField]
    private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlaySFX(string clipName)
    {
        foreach (AudioClipEntry entry in audioSFXData.audioClipEntries)
        {
            if (entry.clipName == clipName)
            {
                AudioSource.PlayClipAtPoint(entry.audioClip, mainCamera.transform.position);
                if (entry.audioTimedClip != null)
                {
                    StartCoroutine(PlayTimedSFX(entry.audioTimedClip, entry.timeToPlayTimedClipSeconds));
                }   
            }
        }
    }

    IEnumerator PlayTimedSFX(AudioClip timedClip, float timeToPlaySeconds)
    {
        yield return new WaitForSeconds(timeToPlaySeconds);
        AudioSource.PlayClipAtPoint(timedClip, mainCamera.transform.position);
    }
}
