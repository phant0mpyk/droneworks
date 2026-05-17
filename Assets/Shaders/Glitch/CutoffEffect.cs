using System;
using UnityEngine;
using UnityEngine.Events;

public class CutoffEffect : MonoBehaviour
{
    [SerializeField] private Material glitch;
    [SerializeField] private Material grain;
    [SerializeField] private float glitchTimer;
    [SerializeField] private float grainTimer;
    [SerializeField] private float noiseMultiplier;
    [SerializeField] private float glitchStrength;
    [SerializeField] private float grainSpeed;
    
    [System.Serializable] public class MyEvent : UnityEvent { }

    public MyEvent onDone;

    
    void Start()
    {
        Time.timeScale = 0;
        glitch.SetFloat("_NoiseMulitplier", noiseMultiplier);
        glitch.SetFloat("_GlitchStrength", glitchStrength);
    }

    // Update is called once per frame
    void Update()
    {
        glitch.SetFloat("_UnscaledTime", Time.unscaledTime);

        if (glitchTimer > 0)
        {
            glitchTimer -= Time.unscaledDeltaTime;
            
        } else if (grainTimer > 0)
        {
            Time.timeScale = 1;
            grainTimer -= Time.unscaledDeltaTime;
            grain.SetFloat("_Speed", grainSpeed);
        }
        else
        {
            onDone.Invoke();
            Destroy(gameObject);
        }
        
        
    }

    private void OnDestroy()
    {
        glitch.SetFloat("_NoiseMulitplier", 0);
        glitch.SetFloat("_GlitchStrength", 0);
        grain.SetFloat("_Speed", 0);
    }
}
