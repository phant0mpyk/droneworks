using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VictimScript : MonoBehaviour
{
    [SerializeField]
    float timeToDieMinutes;
    private float currTimeToDieSeconds = 0f;
    private bool found = false;
    [SerializeField]
    SFXManager sfxManager;
    [SerializeField]
    AudioSFXScriptableObject victimFoundAliveSFXClip;
    [SerializeField]
    AudioSFXScriptableObject victimFoundDeadSFXClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currTimeToDieSeconds += Time.deltaTime;
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            found = true;
            if(currTimeToDieSeconds >= timeToDieMinutes * 60f)
            {
                sfxManager?.PlayVoicelineSFX(victimFoundDeadSFXClip, victimFoundDeadSFXClip.audioClipEntry.clipName, Camera.main.GetComponent<AudioSource>());
            }
            else
            {
                sfxManager?.PlayVoicelineSFX(victimFoundAliveSFXClip, victimFoundAliveSFXClip.audioClipEntry.clipName, Camera.main.GetComponent<AudioSource>());
            }
            return;
        }
    }
}
