using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DescriptionManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Assign the static title IMAGE here (e.g. logo or sprite)")]
    [SerializeField] private Image headingImage; // Changed from TMP_Text to Image

    [Tooltip("Assign the empty text area where the story will type out")]
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private GameObject warningPanel;
    [SerializeField] private Button continueButton;

    [Header("Settings")]
    [TextArea(5, 10)]
    [SerializeField] private string gameDetails = "Navigate Kamil’s warped memories, solve puzzles, and survive hallucinations long enough to escape.";
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // 1. Initial Setup
        if (warningPanel != null) warningPanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(LoadMainMenu);

        // Ensure the Heading Image is visible immediately
        if (headingImage != null)
        {
            headingImage.gameObject.SetActive(true);
        }

        // 2. Start Typing the body text
        StartCoroutine(TypeTextRoutine());
    }

    IEnumerator TypeTextRoutine()
    {
        // Only clear the description body
        if (descriptionText != null)
        {
            descriptionText.text = "";

            foreach (char letter in gameDetails.ToCharArray())
            {
                descriptionText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 3. Wait a moment after typing finishes, then show warning
        yield return new WaitForSeconds(1f);
        ShowWarningPanel();
    }

    void ShowWarningPanel()
    {
        // Hide the description text and heading image so they don't overlap with the warning
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        if (headingImage != null) headingImage.gameObject.SetActive(false);

        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}