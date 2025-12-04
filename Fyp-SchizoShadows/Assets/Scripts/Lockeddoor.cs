using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
    [SerializeField] private AudioClip lockedSound; // Play when trying to open while locked

    private bool isOpening = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private KeyLockedInHand currentKey;

    void Start()
    {
        // Store initial rotation
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // Make sure door has a tag
        if (!gameObject.CompareTag("Door"))
        {
            gameObject.tag = "Door";
            Debug.Log("✓ Added 'Door' tag to " + gameObject.name);
        }
    }

    void Update()
    {
        // Check for key nearby
        if (isLocked)
        {
            CheckForKey();
        }

        // Animate door opening
        if (isOpening && !isOpen)
        {
            AnimateDoorOpen();
        }
    }

    void CheckForKey()
    {
        // Find the key in the scene
        if (currentKey == null)
        {
            KeyLockedInHand[] keys = FindObjectsOfType<KeyLockedInHand>();

            foreach (var key in keys)
            {
                // Check if this is the right key and it's close enough
                float distance = Vector3.Distance(transform.position, key.transform.position);

                if (distance < unlockDistance)
                {
                    currentKey = key;
                    Debug.Log("💡 Key is near the door! Distance: " + distance);

                    // Optional: Show UI prompt "Press [Button] to unlock door"
                    ShowUnlockPrompt(true);
                    break;
                }
            }
        }
        else
        {
            // Key was found, check if it's still close
            float distance = Vector3.Distance(transform.position, currentKey.transform.position);

            if (distance > unlockDistance)
            {
                // Key moved away
                currentKey = null;
                ShowUnlockPrompt(false);
                Debug.Log("Key moved away from door");
            }
        }
    }

    // Call this when player presses unlock button (or automatically)
    public void TryUnlock()
    {
        if (!isLocked)
        {
            Debug.Log("Door is already unlocked!");
            OpenDoor();
            return;
        }

        if (currentKey != null)
        {
            // Check if key is the right one
            float distance = Vector3.Distance(transform.position, currentKey.transform.position);

            if (distance < unlockDistance)
            {
                UnlockDoor();
            }
            else
            {
                Debug.Log("⚠️ Key is too far from door!");
                PlaySound(lockedSound);
            }
        }
        else
        {
            Debug.Log("⚠️ No key found! Door is locked.");
            PlaySound(lockedSound);
        }
    }

    void UnlockDoor()
    {
        isLocked = false;

        Debug.Log("✓ Door unlocked with key!");

        // Play unlock sound
        PlaySound(unlockSound);

        // Unlock the key so player can drop it
        if (currentKey != null)
        {
            currentKey.UnlockKey();
        }

        // Automatically open the door after unlocking
        Invoke(nameof(OpenDoor), 0.5f);
    }

    void OpenDoor()
    {
        if (isLocked)
        {
            Debug.Log("⚠️ Cannot open door - it's locked!");
            PlaySound(lockedSound);
            return;
        }

        if (!isOpen && !isOpening)
        {
            isOpening = true;
            PlaySound(openSound);
            Debug.Log("✓ Opening door...");
        }
    }

    void AnimateDoorOpen()
    {
        // Smoothly rotate door to open position
        transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);

        // Check if door is fully open
        if (Quaternion.Angle(transform.rotation, openRotation) < 1f)
        {
            transform.rotation = openRotation;
            isOpening = false;
            isOpen = true;
            Debug.Log("✓ Door is now fully open!");
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

    void ShowUnlockPrompt(bool show)
    {
        if (show)
        {
            Debug.Log("💡 UI: Press [Button] to unlock door with key");
            // TODO: Show UI text/canvas here
            // Example: unlockPromptUI.SetActive(true);
        }
        else
        {
            Debug.Log("UI: Hide unlock prompt");
            // TODO: Hide UI
            // Example: unlockPromptUI.SetActive(false);
        }
    }

    // Make door interactable with XR
    void OnTriggerEnter(Collider other)
    {
        // If player's controller enters trigger with key
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            if (currentKey != null)
            {
                // Automatically try to unlock when player gets close with key
                TryUnlock();
            }
        }
    }

    // Public methods for external scripts
    public bool IsLocked()
    {
        return isLocked;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
}
