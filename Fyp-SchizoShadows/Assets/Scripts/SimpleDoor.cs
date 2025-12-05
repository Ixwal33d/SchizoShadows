using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleDoor : MonoBehaviour
{
    [Header("Door Animation")]
    public float openAngle = -90f;
    public float openSpeed = 2f;
    
    [Header("Lock Settings")]
    [Tooltip("If checked, the door requires a key with a matching Key ID.")]
    public bool isLocked = false;
    public string requiredKeyID = "MasterKey";
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    public AudioClip unlockSound;
    
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private XRSimpleInteractable interactable;
    
    void Start()
    {
        closedRotation = transform.rotation;
        // Assuming door rotates around Y axis relative to its current rotation
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0); 
        
        // Setup audio
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
             audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        // Setup VR interaction
        SetupVRInteraction();
        
        if (!gameObject.CompareTag("Door"))
        {
            gameObject.tag = "Door"; // Recommended to set door tag for general scene management
        }
        
        Debug.Log($"🚪 Door '{gameObject.name}' initialized - Locked: {isLocked}");
    }
    
    void SetupVRInteraction()
    {
        // Add simple interactable for basic VR hand interaction
        interactable = gameObject.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // Listen for hand interactions to try and open the door
        interactable.selectEntered.AddListener(OnDoorTouched);
    }
    
    void Update()
    {
        // Smooth door animation
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }
    
    public void OnDoorTouched(SelectEnterEventArgs args)
    {
        // Player touched/grabbed door with VR hand
        if (isLocked)
        {
            // Door is LOCKED - cannot open with hand, play locked sound!
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED! Find the key!");
            PlaySound(lockedSound);
        }
        else
        {
            // Door is unlocked - open with hand
            ToggleDoor();
        }
    }
    
    public void ToggleDoor()
    {
        if (isLocked)
        {
            PlaySound(lockedSound);
            return;
        }
        
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    public void OpenDoor()
    {
        if (!isLocked)
        {
            isOpen = true;
            PlaySound(openSound);
        }
    }
    
    public void CloseDoor()
    {
        isOpen = false;
        PlaySound(closeSound);
    }
    
    // Called by key system (EscapeRoomKey.cs)
    public void UnlockWithKey(string keyID)
    {
        if (keyID == requiredKeyID)
        {
            Debug.Log($"✅ Door '{gameObject.name}' UNLOCKED with {keyID}!");
            isLocked = false;
            PlaySound(unlockSound);
            
            // Auto-open after unlocking
            Invoke(nameof(OpenDoor), 0.5f);
        }
        else
        {
            // Should not happen if key script checks first, but here for safety
            Debug.Log($"❌ Wrong key for door '{gameObject.name}'! Needs: {requiredKeyID}");
            PlaySound(lockedSound);
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public getters used by EscapeRoomKey.cs
    public bool IsLocked()
    {
        return isLocked;
    }
    
    public string GetRequiredKeyID()
    {
        return requiredKeyID;
    }
}