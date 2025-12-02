using UnityEngine;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity;

    void Start()
    {
        currentSanity = maxSanity;
    }

    public void DecreaseSanity(float amount)
    {
        currentSanity -= amount * Time.deltaTime;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
        Debug.Log("Sanity Decreasing: " + currentSanity);
    }

    public void IncreaseSanity(float amount)
    {
        currentSanity += amount * Time.deltaTime;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
        Debug.Log("Sanity Increasing: " + currentSanity);
    }

    public float GetSanityPercent()
    {
        return currentSanity / maxSanity;
    }
}
