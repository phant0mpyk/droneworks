using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CapsuleCollider))]
public class FlightRing : MonoBehaviour
{
    [Header("Visual Configurations")]
    [SerializeField] private MeshRenderer ringMeshRenderer;
    [SerializeField] private Material defaultWhiteMaterial;
    [SerializeField] private Material activeGreenMaterial;

    [Header("Timing")]
    [Tooltip("How many seconds should the ring stay green before turning back to white?")]
    [SerializeField] private float greenDurationSeconds = 3f;

    [Header("Audio Feedback (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip passSoundEffect;

    private Coroutine colorResetCoroutine;

    void Start()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.direction = 2;
        }

        if (ringMeshRenderer != null && defaultWhiteMaterial != null)
        {
            ringMeshRenderer.material = defaultWhiteMaterial;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (colorResetCoroutine != null)
            {
                StopCoroutine(colorResetCoroutine);
            }

            colorResetCoroutine = StartCoroutine(RunRingFlashSequence());
        }
    }

    private IEnumerator RunRingFlashSequence()
    {
        if (ringMeshRenderer != null && activeGreenMaterial != null)
        {
            ringMeshRenderer.material = activeGreenMaterial;
        }

        if (audioSource != null && passSoundEffect != null)
        {
            audioSource.PlayOneShot(passSoundEffect);
        }

        yield return new WaitForSeconds(greenDurationSeconds);

        if (ringMeshRenderer != null && defaultWhiteMaterial != null)
        {
            ringMeshRenderer.material = defaultWhiteMaterial;
        }

        colorResetCoroutine = null;
    }
}