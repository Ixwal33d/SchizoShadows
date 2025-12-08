using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

/// <summary>
/// Readable Note/Clue for VR
/// Player grabs it and text appears on screen
/// Now with Voice Over support!
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class ReadableNote : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(3, 10)]
    [SerializeField] private string noteTitle = "Torn Diary Page";
    [TextArea(5, 20)]
    [SerializeField] private string noteText = "The master always hides his important keys in cold places. Check the icebox...no, wait, the bathroom? I can't remember.";

    [Header("Voice Over")]
    [SerializeField] private AudioClip voiceOverClip;       // Kamil's voice reading/reacting to the note
    [SerializeField] private float voiceOverDelay = 0.5f;   // Delay before voice starts
    [SerializeField] private bool playVoiceOnGrab = true;   // Play when grabbed

    [Header("Visual Settings")]
    [SerializeField] private Color noteColor = Color.white;
    [SerializeField] private bool destroyAfterReading = false;
    [SerializeField] private bool canReadMultipleTimes = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip readSound;

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private AudioSource voiceOverSource;    // Separate audio source for voice over
    private bool hasBeenRead = false;
    private bool voiceOverPlayed = false;
    private NoteUIManager uiManager;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Setup audio for pickup/read sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;  // 3D sound

        // Setup separate audio source for voice over (2D - in player's head)
        voiceOverSource = gameObject.AddComponent<AudioSource>();
        voiceOverSource.playOnAwake = false;
        voiceOverSource.spatialBlend = 0f;  // 2D sound (voice in head)
        voiceOverSource.volume = 1f;

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

            // Play voice over
            if (playVoiceOnGrab && voiceOverClip != null)
            {
                if (canReadMultipleTimes || !voiceOverPlayed)
                {
                    StartCoroutine(PlayVoiceOver());
                    voiceOverPlayed = true;
                }
            }
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

        // Stop voice over when note is released (optional - remove if you want voice to continue)
        // if (voiceOverSource.isPlaying)
        // {
        //     voiceOverSource.Stop();
        // }

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

    IEnumerator PlayVoiceOver()
    {
        // Wait for delay
        yield return new WaitForSeconds(voiceOverDelay);

        // Play voice over
        if (voiceOverClip != null && voiceOverSource != null)
        {
            voiceOverSource.clip = voiceOverClip;
            voiceOverSource.Play();
            Debug.Log($"🎙️ Playing voice over for: {noteTitle}");
        }
    }

    void DestroyNote()
    {
        // Stop voice over before destroying
        if (voiceOverSource != null && voiceOverSource.isPlaying)
        {
            voiceOverSource.Stop();
        }

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

    // Stop voice over manually if needed
    public void StopVoiceOver()
    {
        if (voiceOverSource != null && voiceOverSource.isPlaying)
        {
            voiceOverSource.Stop();
        }
    }

    // Play voice over manually (for triggering from other scripts)
    public void TriggerVoiceOver()
    {
        if (voiceOverClip != null)
        {
            StartCoroutine(PlayVoiceOver());
        }
    }
}