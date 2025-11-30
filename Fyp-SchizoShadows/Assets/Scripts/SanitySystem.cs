using UnityEngine;

public class ChangeSanitySystem : MonoBehaviour
{
    public float sanity = 100f;
    public float maxSanity = 100f;

    public void ChangeSanity(float amount)
    {
        sanity = Mathf.Clamp(sanity + amount, 0, maxSanity);
    }

    public bool IsLowSanity()
    {
        return sanity < 40f;
    }
}
