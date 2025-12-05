using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Readable Note/Clue for VR
/// Player grabs it and text appears on screen
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class ReadableNote : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(3, 10)]
    [SerializeField] private string noteTitle = "Torn Diary Page";

    [TextArea(5, 20)]
    [SerializeField] private string noteText = "The master always hides his important keys in cold places. Check the icebox...no, wait, the bathroom? I can't remember.";

    [Header("Visual Settings")]
    [SerializeField] private Color noteColor = Color.white;
    [SerializeField] private bool destroyAfterReading = false;
    [SerializeField] private bool canReadMultipleTimes = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip readSound;

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private bool hasBeenRead = false;
    private NoteUIManager uiManager;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // Find UI manager
        uiManager = FindObjectOfType<NoteUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("⚠️ NoteUIManager not found! Please add NoteUIManager to Canvas.");
        }
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnNoteGrabbed);
        grabInteractable.selectExited.AddListener(OnNoteReleased);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnNoteGrabbed);
        grabInteractable.selectExited.RemoveListener(OnNoteReleased);
    }

    void OnNoteGrabbed(SelectEnterEventArgs args)
    {
        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Show note text
        if (canReadMultipleTimes || !hasBeenRead)
        {
            ShowNote();
            hasBeenRead = true;
        }

        Debug.Log($"📝 Reading note: {noteTitle}");
    }

    void OnNoteReleased(SelectExitEventArgs args)
    {
        // Hide note text when released
        if (uiManager != null)
        {
            uiManager.HideNote();
        }

        // Destroy after reading if set
        if (destroyAfterReading && hasBeenRead)
        {
            Invoke(nameof(DestroyNote), 0.5f);
        }
    }

    void ShowNote()
    {
        if (uiManager != null)
        {
            uiManager.ShowNote(noteTitle, noteText, noteColor);

            // Play read sound
            if (readSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(readSound);
            }
        }
        else
        {
            // Fallback: Show in console if no UI
            Debug.Log($"📜 {noteTitle}:\n{noteText}");
        }
    }

    void DestroyNote()
    {
        Debug.Log($"🗑️ Note '{noteTitle}' destroyed after reading.");
        Destroy(gameObject);
    }

    // Public methods
    public string GetNoteTitle()
    {
        return noteTitle;
    }

    public string GetNoteText()
    {
        return noteText;
    }

    public bool HasBeenRead()
    {
        return hasBeenRead;
    }
}