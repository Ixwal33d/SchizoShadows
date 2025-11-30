using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    public float sanity = 100f;
    public float minSanity = 0f;
    public float maxSanity = 100f;

    // ---------------------------
    //  METHOD TO DECREASE SANITY
    // ---------------------------
    public void DecreaseSanity(float amount)
    {
        sanity -= amount;
        sanity = Mathf.Clamp(sanity, minSanity, maxSanity);
        Debug.Log("Sanity decreased. Current sanity: " + sanity);
    }

    // ---------------------------
    //  METHOD TO INCREASE SANITY
    // ---------------------------
    public void IncreaseSanity(float amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, minSanity, maxSanity);
        Debug.Log("Sanity increased. Current sanity: " + sanity);
    }
}
