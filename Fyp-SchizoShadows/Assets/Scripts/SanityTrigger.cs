using UnityEngine;

public class SanityTrigger : MonoBehaviour
{
    public float sanityDrainAmount = 10f;  // how much sanity to drain
    public bool drainOverTime = false;     
    public float drainRate = 5f;           // per second if draining over time

    private bool playerInside = false;
    private SanityManager sanityManager;

    void Start()
    {
        sanityManager = FindObjectOfType<SanityManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!drainOverTime)
            {
                sanityManager.DecreaseSanity(sanityDrainAmount);
                Debug.Log("Sanity decreased ON ENTER");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    void Update()
    {
        if (drainOverTime && playerInside)
        {
            sanityManager.DecreaseSanity(drainRate * Time.deltaTime);
        }
    }
}
