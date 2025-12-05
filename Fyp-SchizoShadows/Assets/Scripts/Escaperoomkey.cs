using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Collectible Key for Escape Room
/// Player grabs it, tries it on doors until finding the locked one
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class EscapeRoomKey : MonoBehaviour
{
    [Header("Key Identity")]
    [SerializeField] private string keyID = "Key_1";
    [SerializeField] private string keyName = "Old Key";
    [SerializeField] private bool isCorrectKey = false; // ✅ Check ONLY for the ONE correct key
    
    [Header("Visual")]
    [SerializeField] private Color keyColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip wrongKeySound;
    
    [Header("Settings")]
    [SerializeField] private bool lockInHand = true; // Lock in hand until used on door?
    [SerializeField] private float doorDetectDistance = 3f;
    
    private XRGrabInteractable grabInteractable;
    private SubtleGlowKey subtleGlowKey;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool isLockedInHand = false;
    private bool hasBeenPickedUp = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        subtleGlowKey = GetComponent<SubtleGlowKey>();
        keyRigidbody = GetComponent<Rigidbody>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        // Set visual color
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = keyColor;
        }
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;

            if (keyRigidbody != null)
            {
                keyRigidbody.isKinematic = false;
                keyRigidbody.constraints = RigidbodyConstraints.None;
            }

            if (subtleGlowKey != null)
            {
                subtleGlowKey.DisableGlow();
            }

            if (pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            Debug.Log($"✓ Picked up: {keyName}");

            if (lockInHand)
            {
                isLockedInHand = true;
                Debug.Log($"🔒 {keyName} locked in hand! Try it on doors.");
            }
        }
    }

    void Update()
    {
        // Automatically check for nearby locked doors
        if (isLockedInHand && hasBeenPickedUp)
        {
            CheckNearbyLockedDoors();
        }
    }

    void CheckNearbyLockedDoors()
    {
        UniversalDoor[] allDoors = FindObjectsOfType<UniversalDoor>();
        
        foreach (UniversalDoor door in allDoors)
        {
            // Only check locked doors
            if (!door.IsLocked()) continue;
            
            float distance = Vector3.Distance(transform.position, door.transform.position);
            
            // If near a locked door
            if (distance < doorDetectDistance)
            {
                // Automatically try this key
                Debug.Log($"💡 Near locked door! Auto-checking {keyName}...");
                
                if (isCorrectKey)
                {
                    // ✅ CORRECT KEY!
                    bool unlocked = door.UnlockWithKey(keyID);
                    if (unlocked)
                    {
                        Debug.Log($"✅ SUCCESS! {keyName} is the CORRECT key!");
                        isLockedInHand = false; // Can drop now
                        
                        // Force drop the key after unlocking
                        Invoke(nameof(ForceDropKey), 0.5f);
                    }
                }
                else
                {
                    // ❌ WRONG KEY!
                    Debug.Log($"❌ WRONG! {keyName} doesn't work. Dropping key...");
                    if (wrongKeySound != null) audioSource.PlayOneShot(wrongKeySound);
                    
                    // Automatically drop wrong key
                    isLockedInHand = false;
                    Invoke(nameof(ForceDropKey), 0.3f);
                }
                
                return; // Only check one door at a time
            }
        }
    }

    void ForceDropKey()
    {
        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            if (interactor != null)
            {
                (interactor as XRBaseInteractor)?.interactionManager.SelectExit(interactor, grabInteractable);
                Debug.Log($"🔓 {keyName} dropped from hand.");
            }
        }
    }

    public bool IsCorrectKey()
    {
        return isCorrectKey;
    }

    public string GetKeyID()
    {
        return keyID;
    }
}