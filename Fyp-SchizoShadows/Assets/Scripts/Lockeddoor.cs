using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    // These fields are crucial for Inspector setup and KeyLockedInHand to function
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyID = "MainKey";
    [SerializeField] private bool isLocked = true;
    [SerializeField] private float unlockDistance = 2f; 

    [Header("Door Animation")]
    [SerializeField] private bool rotateToOpen = true; 
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Audio")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound; // Used when the wrong key is tried

    private bool isOpening = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);

        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (!gameObject.CompareTag("Door"))
        {
            gameObject.tag = "Door";
        }
    }

    void Update()
    {
        // Animate door opening
        if (isOpening && !isOpen)
        {
            Quaternion targetRotation = openRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            if (Quaternion.Angle(transform.rotation, openRotation) < 1f)
            {
                transform.rotation = openRotation;
                isOpening = false;
                isOpen = true;
            }
        }
    }

    // Called by KeyLockedInHand when player uses the correct key
    public void UnlockDoorWithKey()
    {
        if (!isLocked) return;

        isLocked = false;
        PlaySound(unlockSound);
        Invoke(nameof(OpenDoor), 0.5f); 
    }
    
    // For the 6 simple doors: Opens if unlocked, plays locked sound if locked.
    // Must be linked to the XR Interactable event.
    public void TryOpenSimpleDoor()
    {
        if (!isLocked)
        {
            OpenDoor();
        }
        else
        {
            PlayLockedSound();
        }
    }

    void OpenDoor()
    {
        if (!isOpen && !isOpening)
        {
            isOpening = true;
            PlaySound(openSound);
        }
    }

    // This method is called by KeyLockedInHand when the key is wrong
    public void PlayLockedSound()
    {
        PlaySound(lockedSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Public accessors required by KeyLockedInHand.cs
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
    }
}