using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public void start(){
        SceneManager.LoadSceneAsync("Act1");
        ShowMainButtons();
    }

    [Header("Main Screen")]
    public GameObject mainButtonsPanel; // The group with Play, Options, About, Quit

    [Header("Sub-Menus")]
    public GameObject optionsPanel;     // The Settings screen
    public GameObject aboutPanel;       // The About/Credits screen

    [Header("Scene To Load")]
    public string gameSceneName = "GameLevel";


    // --- NAVIGATION FUNCTIONS ---
    public void GoToMainMenu()
    {
        // 1. IMPORTANT: Unpause the game! 
        // If you don't do this, the Main Menu will be frozen when you load it.
        Time.timeScale = 1f;

        // 2. Load the Menu Scene
        // "0" is usually the index number for the first scene (Main Menu)
        SceneManager.LoadScene("MainMenu");
    }
    public void ShowMainButtons()
    {
        mainButtonsPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        mainButtonsPanel.SetActive(false); // Hide Main
        optionsPanel.SetActive(true);      // Show Options
    }

    public void OpenAbout()
    {
        mainButtonsPanel.SetActive(false); // Hide Main
        aboutPanel.SetActive(true);        // Show About
    }

    // --- ACTION FUNCTIONS ---

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
