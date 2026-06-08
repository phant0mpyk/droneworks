using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CreditsEasterEgg : MonoBehaviour
{
    [System.Serializable]
    public class EasterEggPair
    {
        public string pairName;
        public Texture2D picture;
        public AudioClip soundEffect;
    }

    [Header("UI Component Links")]
    [SerializeField] private RawImage profileImageDisplay;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Easter Egg Pairs Pool")]
    [SerializeField] private List<EasterEggPair> easterEggPool;

    private Texture originalTexture;
    private int lastIndex = -1;
    private Coroutine activeEasterEggCoroutine;

    void Start()
    {
        if (profileImageDisplay != null)
        {
            originalTexture = profileImageDisplay.texture;
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }


    public void TriggerEasterEgg()
    {

        if (easterEggPool == null || easterEggPool.Count == 0) return;

        if (activeEasterEggCoroutine != null)
        {
            StopCoroutine(activeEasterEggCoroutine);
            sfxAudioSource.Stop();
        }

        int randomIndex = GetUniqueRandomIndex(easterEggPool.Count, lastIndex);
        lastIndex = randomIndex;

        EasterEggPair selectedPair = easterEggPool[randomIndex];

        if (selectedPair.picture != null && selectedPair.soundEffect != null)
        {
            activeEasterEggCoroutine = StartCoroutine(RunEasterEggSequence(selectedPair.picture, selectedPair.soundEffect));
        }
    }

    private IEnumerator RunEasterEggSequence(Texture2D newTexture, AudioClip newSound)
    {
        profileImageDisplay.texture = newTexture;
        sfxAudioSource.PlayOneShot(newSound);

        yield return new WaitForSecondsRealtime(newSound.length);

        profileImageDisplay.texture = originalTexture;
        activeEasterEggCoroutine = null;
    }

    private int GetUniqueRandomIndex(int poolCount, int lastIndex)
    {
        if (poolCount <= 1) return 0;

        int newIndex = Random.Range(0, poolCount);
        while (newIndex == lastIndex)
        {
            newIndex = Random.Range(0, poolCount);
        }
        return newIndex;
    }
}