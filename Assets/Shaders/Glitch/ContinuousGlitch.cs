using System;
using UnityEngine;

public class ContinuousGlitch : MonoBehaviour
{
        [SerializeField] private Material glitch;
        [SerializeField] private float noiseMultiplier;
        [SerializeField] private float glitchStrength;
        Vector2 glitchTiling =  new Vector2(0.95f,1);
        Vector2 normalTiling = Vector2.one;
        private bool isOn;

        private void ToggleGlitch(bool turnOn)
        {
            if(turnOn)
            {
                glitch.SetFloat("_NoiseMulitplier", noiseMultiplier);
                glitch.SetFloat("_GlitchStrength", glitchStrength);
                glitch.SetVector("_Tiling", glitchTiling);
                isOn = true;
            }
            else
            {
                glitch.SetFloat("_NoiseMulitplier", 0);
                glitch.SetFloat("_GlitchStrength", 0);
                glitch.SetVector("_Tiling", normalTiling);
                isOn = false;
            }
        }
        
    
        private void OnDestroy()
        {
            glitch.SetFloat("_NoiseMulitplier", 0);
            glitch.SetFloat("_GlitchStrength", 0);
            glitch.SetVector("_Tiling", normalTiling);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "Player")
            {
                ToggleGlitch(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                ToggleGlitch(false);
            }
        }

        private void Update()
        {
            if (isOn)
            {
                glitch.SetFloat("_UnscaledTime", Time.unscaledTime);
            }
        }
}
