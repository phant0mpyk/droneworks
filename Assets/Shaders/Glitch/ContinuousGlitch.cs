using System;
using TMPro;
using UnityEngine;

public class ContinuousGlitch : MonoBehaviour
{
        [SerializeField] private Material glitch;
        [SerializeField] private float noiseMultiplier;
        [SerializeField] private float glitchStrength;
        private TextMeshProUGUI lowSignal;
        private int alphaFlag = 1;
        [SerializeField] private float alphaSpeed;
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

        private void LinkLowSignal()
        {
            if (lowSignal == null)
            {
                lowSignal = GameObject.Find("LowSignal").GetComponent<TextMeshProUGUI>();
            }
        }
        
        private void BlinkAnimation()
        {
            if (lowSignal != null)
            {
                Math.Clamp(lowSignal.alpha += alphaFlag * alphaSpeed * Time.deltaTime, 0, 1);
                if (lowSignal.alpha >= 1 || lowSignal.alpha <= 0)
                {
                    alphaFlag *= -1;
                }
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
                LinkLowSignal();
                if (lowSignal != null)
                {
                    lowSignal.alpha = 1;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                ToggleGlitch(false);
                if (lowSignal != null)
                {
                    lowSignal.alpha = 0;
                }
            }
        }



        private void Update()
        {
            if (isOn)
            {
                glitch.SetFloat("_UnscaledTime", Time.unscaledTime);
                BlinkAnimation();
            }
        }
}
