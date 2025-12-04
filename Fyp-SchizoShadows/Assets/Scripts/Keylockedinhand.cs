using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class KeyLockedInHand : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "MainKey";
    [SerializeField] private AudioClip pickupSound;

    [Header("Lock in Hand Settings")]
    [SerializeField] private bool lockInHandWhenGrabbed = true;
    [SerializeField] private string doorTag = "Door"; // Tag for doors that can be unlocked
    [SerializeField] private float unlockDistance = 2f; // How close to door to unlock

    private XRGrabInteractable grabInteractable;
    private SubtleGlowKey subtleGlowKey;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool isLockedInHand = false;
    private bool hasBeenPickedUp = false;
    private GameObject nearestDoor;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        subtleGlowKey = GetComponent<SubtleGlowKey>();
        keyRigidbody = GetComponent<Rigidbody>();

        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
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

            // Make key non-kinematic
            if (keyRigidbody != null)
            {
                keyRigidbody.isKinematic = false;
                keyRigidbody.constraints = RigidbodyConstraints.None;
            }

            // Stop glow effect
            if (subtleGlowKey != null)
            {
                subtleGlowKey.DisableGlow();
            }

            // Play pickup sound
            if (pickupSound != null)
            {
                audioSource.clip = pickupSound;
                audioSource.Play();
            }

            Debug.Log("✓ Key picked up: " + keyID);

            // LOCK THE KEY IN HAND - Player cannot drop it now!
            if (lockInHandWhenGrabbed)
            {
                LockInHand(true);
                Debug.Log("🔒 Key is now LOCKED in your hand! Find a door to use it.");
            }
        }
    }

    void OnReleaseAttempt(SelectExitEventArgs args)
    {
        // This is called when player tries to release the key

        if (isLockedInHand)
        {
            // Check if player is near a door
            if (IsNearDoor())
            {
                // Allow release - player is at a door
                Debug.Log("✓ Key released near door - can unlock!");
                isLockedInHand = false;
                // Key will naturally drop/release here
            }
            else
            {
                // Prevent release - re-grab the key immediately!
                Debug.Log("⚠️ Cannot drop key yet! You must use it on a door first.");

                // Force re-grab after a tiny delay
                Invoke(nameof(ForceReGrab), 0.05f);
            }
        }
    }

    void ForceReGrab()
    {
        // Find the interactor that just released this
        if (!grabInteractable.isSelected)
        {
            // Try to find nearby interactor and force grab again
            var interactors = FindObjectsOfType<XRBaseInteractor>();

            foreach (var interactor in interactors)
            {
                // Check if this interactor is close enough
                if (Vector3.Distance(interactor.transform.position, transform.position) < 0.5f)
                {
                    // Force the interactor to select this object again
                    interactor.interactionManager.SelectEnter(interactor, grabInteractable);
                    Debug.Log("🔒 Key re-grabbed! Cannot drop until near door.");
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Check for nearby doors while key is held
        if (isLockedInHand && hasBeenPickedUp)
        {
            FindNearestDoor();
        }
    }

    void FindNearestDoor()
    {
        // Find all objects tagged as "Door"
        GameObject[] doors = GameObject.FindGameObjectsWithTag(doorTag);

        if (doors.Length == 0)
        {
            nearestDoor = null;
            return;
        }

        float closestDistance = float.MaxValue;
        GameObject closest = null;

        foreach (GameObject door in doors)
        {
            float distance = Vector3.Distance(transform.position, door.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = door;
            }
        }

        nearestDoor = closest;

        // Optional: Show UI hint when near door
        if (closestDistance < unlockDistance)
        {
            Debug.Log("💡 Near door! You can release/use the key now.");
            // You can trigger UI here: "Press [Button] to unlock door"
        }
    }

    bool IsNearDoor()
    {
        if (nearestDoor == null)
        {
            FindNearestDoor();
        }

        if (nearestDoor != null)
        {
            float distance = Vector3.Distance(transform.position, nearestDoor.transform.position);
            return distance < unlockDistance;
        }

        return false;
    }

    void LockInHand(bool locked)
    {
        isLockedInHand = locked;

        if (locked)
        {
            Debug.Log("🔒 Key locked in hand - cannot drop!");
        }
        else
        {
            Debug.Log("🔓 Key unlocked - can drop now");
        }
    }

    // Public method to unlock key manually (call this from door script)
    public void UnlockKey()
    {
        LockInHand(false);
    }

    // Check if key is locked in hand
    public bool IsLockedInHand()
    {
        return isLockedInHand;
    }

    // Get the door this key can unlock
    public GameObject GetNearestDoor()
    {
        return nearestDoor;
    }
}
