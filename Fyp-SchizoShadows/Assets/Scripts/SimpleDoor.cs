using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Simple VR Door with Key Support
/// Combines your SimpleDoor with VR interaction and key system
/// </summary>
public class SimpleDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = -90f;
    public float openSpeed = 2f;
    
    [Header("Lock Settings")]
    public bool isLocked = false;
    public string requiredKeyID = "MasterKey";
    
    [Header("Interaction Settings")]
    public bool autoOpenNearPlayer = false;
    public float autoOpenDistance = 2f;
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    public AudioClip unlockSound;
    
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Transform player;
    private AudioSource audioSource;
    private XRSimpleInteractable interactable;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        
        // Find player
        GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCam != null)
        {
            player = mainCam.transform;
        }
        
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        // Setup VR interaction (optional - for hand push/pull)
        SetupVRInteraction();
        
        Debug.Log($"🚪 Door '{gameObject.name}' initialized - Locked: {isLocked}");
    }
    
    void SetupVRInteraction()
    {
        // Add simple interactable for VR hands
        interactable = gameObject.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // Listen for hand interactions
        interactable.selectEntered.AddListener(OnDoorTouched);
    }
    
    void Update()
    {
        // Smooth door animation
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
        
        // Auto-open near player (optional)
        if (autoOpenNearPlayer && !isLocked && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < autoOpenDistance && !isOpen)
            {
                OpenDoor();
            }
            else if (distance >= autoOpenDistance && isOpen)
            {
                CloseDoor();
            }
        }
    }
    
    void OnDoorTouched(SelectEnterEventArgs args)
    {
        // Player touched/grabbed door with VR hand
        if (isLocked)
        {
            // Door is LOCKED - cannot open with hand!
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED! Find the key!");
            PlaySound(lockedSound);
            // Do NOT toggle - locked doors need key!
        }
        else
        {
            // Door is unlocked - can open with hand
            ToggleDoor();
        }
    }
    
    public void ToggleDoor()
    {
        // Only works if door is unlocked
        if (isLocked)
        {
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED! Need key: {requiredKeyID}");
            PlaySound(lockedSound);
            return;
        }
        
        // Toggle open/close
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
            Debug.Log($"🚪 Opening door '{gameObject.name}'");
        }
    }
    
    public void CloseDoor()
    {
        isOpen = false;
        PlaySound(closeSound);
        Debug.Log($"🚪 Closing door '{gameObject.name}'");
    }
    
    // Called by key system
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
    
    // Public getters
    public bool IsLocked()
    {
        return isLocked;
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }
    
    public string GetRequiredKeyID()
    {
        return requiredKeyID;
    }
    
    // For key system integration
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
}