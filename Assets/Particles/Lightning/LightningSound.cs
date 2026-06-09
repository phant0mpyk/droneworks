using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightningSound : MonoBehaviour
{
    private ParticleSystem particles;
    List<ParticleSystem.Particle> parts = new();
    [SerializeField] private GameObject soundObjPrefab;
    [SerializeField] private AudioClip[] sounds;
    void Start()
    {
        particles =  GetComponent<ParticleSystem>();
    }

    
    IEnumerator SpawnLightningSound(Vector3 position)
    {
        GameObject newSoundObj = Instantiate(soundObjPrefab, position, Quaternion.identity);
        yield return new WaitForSeconds(3f);
        AudioSource source = newSoundObj.GetComponent<AudioSource>();
        source.clip = sounds[Random.Range(0, sounds.Length)];
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(newSoundObj);
    }

    private void OnParticleTrigger()
    {
        particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, parts, out var colliderData);

        
        for (int i = 0; i < parts.Count; ++i)
        {
            StartCoroutine(SpawnLightningSound(parts[i].position));

        }
        parts.Clear();
    }

    
    
}
