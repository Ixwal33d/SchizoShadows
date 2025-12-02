using UnityEngine;

public class PlayerSanityTrigger : MonoBehaviour
{
    public SanityManager sanityManager;
    public float drainRate = 10f;
    public float restoreRate = 5f;

    bool isPlayerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("Entered Sanity Zone");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("Exited Sanity Zone");
        }
    }

    void Update()
    {
        if (sanityManager == null) return;

        if (isPlayerInside)
        {
            sanityManager.DecreaseSanity(drainRate);
        }
        else
        {
            sanityManager.IncreaseSanity(restoreRate);
        }
    }
}
