using System.Collections;
using UnityEngine;

public class AudioLoopWithDelay : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    [Range(0f, 30f)]
    public float minDelay = 10f;
    [Range(0f, 30f)]
    public float maxDelay = 15f;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Make sure these are off — we control playback manually
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        StartCoroutine(LoopSound());
    }

    private IEnumerator LoopSound()
    {
        while (true)
        {
            audioSource.PlayOneShot(clip);
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(clip.length + delay);
        }
    }
}