using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRKeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "MainKey";
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;

    [Header("Hold Settings")]
    [SerializeField] private bool requireButtonHold = false; // If true, player must hold button to keep key

    private XRGrabInteractable grabInteractable;
    private SubtleGlowKey subtleGlowKey;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool isBeingHeld = false;
    private bool hasBeenPickedUpOnce = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        subtleGlowKey = GetComponent<SubtleGlowKey>();
        keyRigidbody = GetComponent<Rigidbody>();

        // Setup audio source for pickup/drop sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isBeingHeld = true;

        // First time picking up the key
        if (!hasBeenPickedUpOnce)
        {
            hasBeenPickedUpOnce = true;

            // Make the key non-kinematic so it can be held and moved
            if (keyRigidbody != null)
            {
                keyRigidbody.isKinematic = false;
                // Unfreeze constraints if they were set
                keyRigidbody.constraints = RigidbodyConstraints.None;
            }

            // Stop the glow effect when first picked up
            if (subtleGlowKey != null)
            {
                subtleGlowKey.DisableGlow();
            }

            Debug.Log("✓ Key picked up for the first time: " + keyID);
        }
        else
        {
            Debug.Log("✓ Key grabbed again: " + keyID);
        }

        // Play pickup sound
        if (pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }

        // You can add your own inventory system call here
        // Example: InventoryManager.Instance.AddKey(keyID);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;

        Debug.Log("✓ Key released: " + keyID);

        // Play drop sound
        if (dropSound != null)
        {
            audioSource.clip = dropSound;
            audioSource.Play();
        }

        // Optional: You can add logic here for when key is dropped
        // Example: Check if key was dropped in the right place
    }

    // Public method to check if key is currently being held
    public bool IsHeld()
    {
        return isBeingHeld;
    }

    // Public method to force drop the key (if needed for game logic)
    public void ForceRelease()
    {
        if (grabInteractable.isSelected)
        {
            // Get the interactor that's holding this object
            var interactor = grabInteractable.firstInteractorSelecting;
            if (interactor != null)
            {
                // Force the interactor to drop it
                (interactor as XRBaseInteractor)?.interactionManager.SelectExit(interactor, grabInteractable);
            }
        }
    }

    // Optional: Prevent key from being dropped (lock it in hand)
    public void LockInHand(bool locked)
    {
        if (locked)
        {
            // Disable the ability to drop the key
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
        else
        {
            // Re-enable dropping
            if (!grabInteractable.selectExited.GetPersistentEventCount().Equals(0))
            {
                grabInteractable.selectExited.AddListener(OnReleased);
            }
        }
    }
}
