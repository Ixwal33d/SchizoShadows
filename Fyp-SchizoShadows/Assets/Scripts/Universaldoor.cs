using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Universal Door Script
/// - Unlocked doors: Open with HAND interaction (grab/push)
/// - Locked door: Only opens with CORRECT KEY
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class UniversalDoor : MonoBehaviour
{
    [Header("Door State")]
    [SerializeField] private bool isLocked = false; // ✅ Check this ONLY for the ONE locked door!
    [SerializeField] private string requiredKeyID = "CorrectKey"; // Which key opens this door
    
    [Header("Door Animation")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 200f; // Speed for physics-based opening
    [SerializeField] private Vector3 hingeAxis = Vector3.up; // Rotation axis
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound; // When trying locked door with hand
    [SerializeField] private AudioClip unlockSound;
    
    private bool isOpen = false;
    private XRGrabInteractable grabInteractable;
    private HingeJoint hingeJoint;
    private Rigidbody doorRigidbody;
    private AudioSource audioSource;

    void Start()
    {
        Setup();
    }

    void Setup()
    {
        // Get components
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        // Setup physics-based door
        SetupPhysicsDoor();
        
        // Configure interaction
        if (isLocked)
        {
            // Locked door - disable hand grabbing
            grabInteractable.enabled = false;
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED! Needs key: {requiredKeyID}");
        }
        else
        {
            // Unlocked door - enable hand grabbing
            grabInteractable.enabled = true;
            Debug.Log($"✅ Door '{gameObject.name}' is UNLOCKED - can open with hand!");
        }
        
        // Listen for grab attempts on locked door
        grabInteractable.selectEntered.AddListener(OnDoorGrabbed);
    }

    void SetupPhysicsDoor()
    {
        // Add Rigidbody for physics
        doorRigidbody = GetComponent<Rigidbody>();
        if (doorRigidbody == null)
        {
            doorRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        doorRigidbody.mass = 20f; // Heavy door
        doorRigidbody.drag = 5f;
        doorRigidbody.angularDrag = 5f;
        doorRigidbody.useGravity = false;
        
        if (isLocked)
        {
            doorRigidbody.isKinematic = true; // Can't move when locked
        }
        else
        {
            doorRigidbody.isKinematic = false; // Can move with hand
        }
        
        // Add Hinge Joint for rotation
        hingeJoint = GetComponent<HingeJoint>();
        if (hingeJoint == null)
        {
            hingeJoint = gameObject.AddComponent<HingeJoint>();
        }
        
        // Configure hinge
        hingeJoint.axis = hingeAxis;
        hingeJoint.useLimits = true;
        
        JointLimits limits = hingeJoint.limits;
        limits.min = 0f;
        limits.max = openAngle;
        hingeJoint.limits = limits;
    }

    void OnDoorGrabbed(SelectEnterEventArgs args)
    {
        if (isLocked)
        {
            // Trying to open locked door with hand!
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED! Find the key!");
            PlaySound(lockedSound);
            
            // Prevent grab
            Invoke(nameof(ForceRelease), 0.1f);
        }
        else
        {
            // Unlocked door - allow opening
            Debug.Log($"🚪 Opening door '{gameObject.name}' with hand...");
            PlaySound(openSound);
            isOpen = true;
        }
    }

    void ForceRelease()
    {
        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            if (interactor != null)
            {
                (interactor as XRBaseInteractor)?.interactionManager.SelectExit(interactor, grabInteractable);
            }
        }
    }

    // Unlock door with key
    public bool UnlockWithKey(string keyID)
    {
        if (!isLocked)
        {
            Debug.Log($"💡 Door '{gameObject.name}' is already unlocked! Just grab it with hand.");
            return false;
        }

        if (keyID == requiredKeyID)
        {
            // CORRECT KEY!
            Debug.Log($"✅ Door '{gameObject.name}' UNLOCKED with {keyID}!");
            isLocked = false;
            
            // Enable hand interaction now
            grabInteractable.enabled = true;
            doorRigidbody.isKinematic = false;
            
            PlaySound(unlockSound);
            
            // Auto open slightly
            Invoke(nameof(OpenDoorSlightly), 0.5f);
            return true;
        }
        else
        {
            // WRONG KEY!
            Debug.Log($"❌ Wrong key for door '{gameObject.name}'! Needs: {requiredKeyID}, Got: {keyID}");
            PlaySound(lockedSound);
            return false;
        }
    }

    void OpenDoorSlightly()
    {
        // Push door open a bit after unlocking
        if (doorRigidbody != null)
        {
            doorRigidbody.AddRelativeTorque(Vector3.up * openSpeed);
        }
        PlaySound(openSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public methods
    public bool IsLocked()
    {
        return isLocked;
    }

    public string GetRequiredKeyID()
    {
        return requiredKeyID;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        grabInteractable.enabled = !locked;
        
        if (doorRigidbody != null)
        {
            doorRigidbody.isKinematic = locked;
        }
    }
}