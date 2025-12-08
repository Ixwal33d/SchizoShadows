using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoScreenCanvas; // The UI Canvas holding the RawImage
    public string gameSceneName; // The name of your actual game scene

    void Start()
    {
        // Hide the screen initially so it doesn't block the menu
        videoScreenCanvas.SetActive(false);

        // Prepare the video so it doesn't lag when starting
        videoPlayer.Prepare();

        // Listen for when the video finishes
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    // Call this function from your "Load" Button's OnClick event
    public void PlayCutscene()
    {
        videoScreenCanvas.SetActive(true); // Show the screen
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Video is done, load the actual game
        SceneManager.LoadScene(gameSceneName);
    }
}