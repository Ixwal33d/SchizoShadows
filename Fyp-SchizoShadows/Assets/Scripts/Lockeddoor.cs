using UnityEngine;

public class LockedDoor : MonoBehaviour
{
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
    [SerializeField] private AudioClip lockedSound;

    private bool isOpening = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    void Start()
    {
        // Store rotations
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // Add Door tag if missing
        if (!gameObject.CompareTag("Door"))
        {
            gameObject.tag = "Door";
            Debug.Log("✓ Added 'Door' tag to " + gameObject.name);
        }

        if (isLocked)
        {
            Debug.Log($"🔒 Door '{gameObject.name}' is LOCKED!");
        }
        else
        {
            Debug.Log($"✓ Door '{gameObject.name}' is UNLOCKED");
        }
    }

    void Update()
    {
        // Animate door opening
        if (isOpening && !isOpen)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);

            if (Quaternion.Angle(transform.rotation, openRotation) < 1f)
            {
                transform.rotation = openRotation;
                isOpening = false;
                isOpen = true;
                Debug.Log($"✅ Door '{gameObject.name}' is now fully OPEN!");
            }
        }
    }

    // Called by KeyLockedInHand when player uses the key
    public void UnlockDoorWithKey()
    {
        if (!isLocked)
        {
            Debug.Log("💡 This door is already unlocked!");
            return;
        }

        isLocked = false;

        Debug.Log($"🔓 Door '{gameObject.name}' UNLOCKED with key!");

        // Play unlock sound
        PlaySound(unlockSound);

        // Open the door after short delay
        Invoke(nameof(OpenDoor), 0.5f);
    }

    void OpenDoor()
    {
        if (!isOpen && !isOpening)
        {
            isOpening = true;
            PlaySound(openSound);
            Debug.Log($"🚪 Opening door '{gameObject.name}'...");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
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
    }
}
