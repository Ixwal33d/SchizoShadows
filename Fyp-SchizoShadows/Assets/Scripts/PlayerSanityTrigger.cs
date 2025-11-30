using UnityEngine;

public class PlayerSanityTrigger : MonoBehaviour
{
    public SanitySystem sanitySystem;
    public float sanityLossAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sanitySystem.DecreaseSanity(sanityLossAmount);
        }
    }
}
