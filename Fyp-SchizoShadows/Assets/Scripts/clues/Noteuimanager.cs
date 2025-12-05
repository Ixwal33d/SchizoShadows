using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI for displaying readable notes
/// Add this to your Canvas
/// </summary>
public class NoteUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private Text noteTitleText;
    [SerializeField] private Text noteContentText;
    [SerializeField] private Image noteBackground;

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private float fadeProgress = 0f;

    void Awake()
    {
        // Get or add canvas group for fading
        canvasGroup = notePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notePanel.AddComponent<CanvasGroup>();
        }

        // Start hidden
        HideNote();

        Debug.Log("✅ NoteUIManager ready!");
    }

    void Update()
    {
        // Handle fade animations
        if (isShowing && fadeProgress < 1f)
        {
            fadeProgress += Time.deltaTime / fadeInDuration;
            canvasGroup.alpha = Mathf.Clamp01(fadeProgress);
        }
        else if (!isShowing && fadeProgress > 0f)
        {
            fadeProgress -= Time.deltaTime / fadeOutDuration;
            canvasGroup.alpha = Mathf.Clamp01(fadeProgress);

            if (fadeProgress <= 0f)
            {
                notePanel.SetActive(false);
            }
        }
    }

    public void ShowNote(string title, string content, Color backgroundColor)
    {
        // Set text
        if (noteTitleText != null)
        {
            noteTitleText.text = title;
        }

        if (noteContentText != null)
        {
            noteContentText.text = content;
        }

        // Set background color
        if (noteBackground != null)
        {
            noteBackground.color = backgroundColor;
        }

        // Show panel
        notePanel.SetActive(true);
        isShowing = true;
        fadeProgress = 0f;

        Debug.Log($"📝 Showing note: {title}");
    }

    public void HideNote()
    {
        isShowing = false;

        Debug.Log("📝 Hiding note");
    }

    public bool IsNoteVisible()
    {
        return isShowing && notePanel.activeSelf;
    }
}
