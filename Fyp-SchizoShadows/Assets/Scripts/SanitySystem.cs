using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    public float maxSanity = 100f;
    public float currentSanity;

    void Start()
    {
        currentSanity = maxSanity;
    }

    public void DecreaseSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
    }

    public void IncreaseSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
    }

    public float GetSanityPercent()
    {
        return currentSanity / maxSanity;
    }
}
