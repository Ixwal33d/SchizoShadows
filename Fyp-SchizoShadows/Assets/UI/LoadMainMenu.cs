using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMainMenu : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image loadingImage;
    [SerializeField] private GameObject phantomPawsSplash; // Reference to the PhantomPaws image object
    [SerializeField] private float fakeLoadDelay = 2f;

    void Start()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    IEnumerator LoadMainSceneAsync()
    {
        // --- 1. Show PhantomPaws Splash for 5 seconds ---
        if (phantomPawsSplash != null)
        {
            phantomPawsSplash.SetActive(true);

            // Ensure loading bar/image are hidden while splash is visible
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (loadingImage != null) loadingImage.gameObject.SetActive(false);
        }

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        if (phantomPawsSplash != null)
        {
            phantomPawsSplash.SetActive(false);

            // Re-enable (unhide) the loading UI automatically
            if (progressBar != null) progressBar.gameObject.SetActive(true);
            if (loadingImage != null) loadingImage.gameObject.SetActive(true);
        }

        // --- 2. Start the actual loading sequence ---
        yield return new WaitForSeconds(fakeLoadDelay);

        AsyncOperation operation = SceneManager.LoadSceneAsync("Description");
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;

            // Update the image fill amount
            if (loadingImage != null)
            {
                loadingImage.fillAmount = progress;
            }

            // Check if the load has finished
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}