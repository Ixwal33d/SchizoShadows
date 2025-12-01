using UnityEngine;

public class PlayerSanityTrigger : MonoBehaviour
{
    public SanitySystem sanitySystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered sanity zone");
            sanitySystem.DecreaseSanity(10f);
        }
    }
}
