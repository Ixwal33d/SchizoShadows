using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SanityTrigger : MonoBehaviour
{
    [Header("Drop settings")]
    public float dropAmount = 10f;            // how much to subtract on trigger
    public bool dropOnlyOncePerEntry = true;  // true = one drop each time player enters (default)
    public bool dropOnlyOnceEver = false;     // true = after first ever drop this trigger becomes inactive

    bool playerInside = false;
    bool dropAppliedThisEntry = false;
    bool dropAppliedEver = false;
    SanityManager sanityManager;

    void Start()
    {
        sanityManager = FindObjectOfType<SanityManager>();
        if (sanityManager == null)
            Debug.LogError("SanityTrigger: No SanityManager found in scene.");
        var c = GetComponent<Collider>();
        if (!c.isTrigger) Debug.LogWarning($"{name}: Collider.isTrigger is false — set it to true.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (dropOnlyOnceEver && dropAppliedEver) return;

        if (!dropOnlyOncePerEntry || !dropAppliedThisEntry)
        {
            sanityManager?.DecreaseSanity(dropAmount);
            dropAppliedThisEntry = true;
            dropAppliedEver = true;
            Debug.Log($"{name}: Sanity dropped by {dropAmount} on entry.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        // reset per‑entry flag so re‑entering will drop again (if dropOnlyOncePerEntry = true)
        dropAppliedThisEntry = false;
    }

    // Optional: if you want this trigger to become inert after a permanent drop:
    public void ResetTrigger()
    {
        dropAppliedThisEntry = false;
        dropAppliedEver = false;
    }
}
