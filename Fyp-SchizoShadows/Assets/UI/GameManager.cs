using UnityEngine;
using UnityEngine.SceneManagement; // Needed to reload scene

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel; // Drag your Panel object here

    void Start()
    {
        // Ensure the panel is hidden when the game starts
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        gameOverPanel.SetActive(false); // Force it to hide
    }

    // Call this function when the Ghost catches the player
    public void TriggerGameOver()
    {
        // 1. Show the cursor/panel
        gameOverPanel.SetActive(true);

        // 2. Pause the game (Optional, stops physics)
        Time.timeScale = 0f;

        // 3. Move panel in front of player (Optional polish)
        // transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2;
        // transform.LookAt(Camera.main.transform);
    }
   
    public void GoToMainMenu()
    {
        // 1. IMPORTANT: Unpause the game! 
        // If you don't do this, the Main Menu will be frozen when you load it.
        Time.timeScale = 1f;

        // 2. Load the Menu Scene
        // "0" is usually the index number for the first scene (Main Menu)
        SceneManager.LoadScene("MainMenu");
    }
    public void RestartGame()
    {
        // Unpause time before reloading!
        Time.timeScale = 1f;
        // Reloads the current open scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}