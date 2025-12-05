using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class KeyLockedInHand : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "MainKey";
    [SerializeField] private AudioClip pickupSound;

    [Header("Unlock Check Settings")]
    [SerializeField] private string doorTag = "Door"; 
    [SerializeField] private float unlockDistance = 2f; // Distance from door to check key

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool hasBeenPickedUp = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        keyRigidbody = GetComponent<Rigidbody>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        // The check happens when the player releases the key (Select Exited)
        grabInteractable.selectExited.AddListener(OnReleaseAttempt);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleaseAttempt);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;

            if (keyRigidbody != null)
            {
                // Ensure key is non-kinematic to allow throwing/dropping
                keyRigidbody.isKinematic = false;
                keyRigidbody.constraints = RigidbodyConstraints.None;
            }

            if (pickupSound != null)
            {
                audioSource.clip = pickupSound;
                audioSource.Play();
            }
        }
    }

    void OnReleaseAttempt(SelectExitEventArgs args)
    {
        // Find all doors tagged "Door"
        GameObject[] doors = GameObject.FindGameObjectsWithTag(doorTag);

        foreach (GameObject doorObj in doors)
        {
            float distance = Vector3.Distance(transform.position, doorObj.transform.position);

            if (distance < unlockDistance)
            {
                LockedDoor doorScript = doorObj.GetComponent<LockedDoor>();

                if (doorScript != null && doorScript.IsLocked())
                {
                    // Check if key ID matches the required door ID
                    if (doorScript.GetRequiredKeyID() == keyID)
                    {
                        // ✅ CORRECT KEY!
                        doorScript.UnlockDoorWithKey();
                        Debug.Log($"✅ {keyID} unlocked the correct door! Key destroyed.");
                        Destroy(gameObject); // Destroy the key after use
                        return; 
                    }
                    else
                    {
                        // ❌ WRONG KEY! Key is already dropping, give player feedback.
                        doorScript.PlayLockedSound(); 
                        Debug.Log($"❌ Wrong key ({keyID})! This door needs: {doorScript.GetRequiredKeyID()}. Key dropped.");
                        // The key automatically drops since we don't force re-grab.
                        return; 
                    }
                }
            }
        }
    }
    
    // Public accessor
    public string GetKeyID()
    {
        return keyID;
    }
}