using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class EscapeRoomKey : MonoBehaviour
{
    [Header("Key Identity")]
    [Tooltip("Must match the door's Required Key ID to unlock it.")]
    [SerializeField] private string keyID = "MasterKey";
    [SerializeField] private string keyName = "Old Key";
    
    [Header("Visual")]
    [SerializeField] private Color keyColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip wrongKeySound;
    
    [Header("Settings")]
    [Tooltip("The distance (in meters) the key must be from the door to attempt a check.")]
    [SerializeField] private float doorDetectDistance = 3f;
    
    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool isLockedInHand = false;
    private bool hasBeenPickedUp = false;

    void Awake()
    {
        Debug.Log($"🔑 Key '{keyName}' initializing...");
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        keyRigidbody = GetComponent<Rigidbody>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = keyColor;
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log($"✋ {keyName} GRABBED! KeyID: {keyID}");
        
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;
        }

        if (keyRigidbody != null)
        {
            // Ensure key is non-kinematic to allow throwing/dropping
            keyRigidbody.isKinematic = false;
            keyRigidbody.constraints = RigidbodyConstraints.None;
        }

        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Key is immediately locked in hand once grabbed
        isLockedInHand = true; 
        Debug.Log($"🔒 {keyName} LOCKED in hand!");
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        // If the key is locked in hand, try to re-grab it immediately (to prevent dropping)
        if (isLockedInHand)
        {
            Invoke(nameof(TryReGrab), 0.1f);
        }
    }
    
    void TryReGrab()
    {
        // Only attempt re-grab if the key is still locked in hand AND not currently selected by an interactor
        if (!grabInteractable.isSelected && isLockedInHand)
        {
            // Find the closest interactor to re-grab the key
            XRBaseInteractor[] interactors = FindObjectsOfType<XRBaseInteractor>();
            
            foreach (var interactor in interactors)
            {
                float distance = Vector3.Distance(interactor.transform.position, transform.position);
                if (distance < 0.5f) // Close enough to be the original interactor
                {
                    interactor.interactionManager.SelectEnter(interactor, grabInteractable);
                    Debug.Log($"✅ {keyName} re-grabbed by interactor!");
                    return;
                }
            }
        }
    }

    void Update()
    {
        // Only run the lock check if the key is currently held/selected by the player
        if (isLockedInHand && grabInteractable.isSelected)
        {
            CheckNearbyLockedDoors();
        }
    }

    void CheckNearbyLockedDoors()
    {
        // NOTE: We are now checking for SimpleDoor instead of LockedDoor
        SimpleDoor[] allDoors = FindObjectsOfType<SimpleDoor>();
        
        if (allDoors.Length == 0)
        {
            Debug.LogWarning("⚠️ No SimpleDoor doors found in scene!");
            return;
        }
        
        foreach (SimpleDoor door in allDoors)
        {
            // Check 1: Is the door locked?
            if (!door.IsLocked()) continue;
            
            // Check 2: Is the key close enough?
            float distance = Vector3.Distance(transform.position, door.transform.position);
            
            if (distance < doorDetectDistance)
            {
                Debug.Log($"💡 {keyName} near locked door! Door Needs: {door.GetRequiredKeyID()}");
                
                // Check 3: Does the Key ID MATCH the Door ID? (Simplified logic)
                if (keyID == door.GetRequiredKeyID())
                {
                    Debug.Log($"✅ CORRECT KEY! {keyName} unlocks door!");
                    door.UnlockWithKey(keyID);
                    isLockedInHand = false; // Key is no longer needed
                    ForceDropKey();
                    Destroy(gameObject, 0.5f); // Destroy the key after use
                    return;
                }
                else
                {
                    Debug.Log($"❌ WRONG KEY! {keyName} doesn't work! Dropping key.");
                    if (wrongKeySound != null) audioSource.PlayOneShot(wrongKeySound);
                    isLockedInHand = false; // Player is forced to drop the key
                    ForceDropKey();
                    return;
                }
            }
        }
    }

    void ForceDropKey()
    {
        Debug.Log($"🔓 Forcing drop of {keyName}...");
        
        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting as XRBaseInteractor;
            if (interactor != null)
            {
                // Force the interactor to exit selection, releasing the key
                interactor.interactionManager.SelectExit(interactor, grabInteractable);
                Debug.Log($"✅ {keyName} dropped!");
            }
        }
        
        // This brief coroutine prevents the key from being immediately re-grabbed by the hand that just dropped it.
        StartCoroutine(TemporaryDisableGrab());
    }
    
    IEnumerator TemporaryDisableGrab()
    {
        var originalMask = grabInteractable.interactionLayers;
        grabInteractable.interactionLayers = 0; // Disable interaction temporarily
        
        yield return new WaitForSeconds(0.5f); // Wait half a second
        
        grabInteractable.interactionLayers = originalMask; // Restore interaction
        Debug.Log($"✅ {keyName} can be grabbed again (if it wasn't destroyed)!");
    }

    public string GetKeyID()
    {
        return keyID;
    }
}