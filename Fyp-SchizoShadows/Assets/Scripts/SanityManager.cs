using UnityEngine;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity;

    void Awake()
    {
        currentSanity = maxSanity;
    }

    // one-time immediate decrease (call with amount, e.g. 10f or with delta*rate)
    public void DecreaseSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        Debug.Log($"Sanity Decreased → {currentSanity:0.0}/{maxSanity}");
    }

    // immediate or per-frame increase (call with amount, or amount * Time.deltaTime for per-second)
    public void IncreaseSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        Debug.Log($"Sanity Increased → {currentSanity:0.0}/{maxSanity}");
    }

    public float GetSanityPercent()
    {
        if (maxSanity <= 0f) return 0f;
        return currentSanity / maxSanity;
    }
}

