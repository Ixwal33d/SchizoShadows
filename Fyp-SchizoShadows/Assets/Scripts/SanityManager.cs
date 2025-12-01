using UnityEngine;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity;
    public float sanityDrainRate = 2f;   // sanity lost per second

    [Header("Player Reference (Camera)")]
    public Transform playerCamera;  // drag Main Camera here

    void Start()
    {
        currentSanity = maxSanity;

        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        DrainSanityOverTime();
    }

    void DrainSanityOverTime()
    {
        currentSanity -= sanityDrainRate * Time.deltaTime;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
    }

    public float GetSanityPercent()
    {
        return currentSanity / maxSanity;
    }
}
