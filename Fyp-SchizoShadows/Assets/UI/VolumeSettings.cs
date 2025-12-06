using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Components")]
    public AudioMixer myMixer;
    public Slider mySlider;

    [Header("Settings")]
    // This must match the name you typed in the Audio Mixer EXACTLY
    public string parameterName = "MasterVolume";

    void Start()
    {
        // 1. Load saved volume (or default to 1.0 if no save exists)
        float savedVolume = PlayerPrefs.GetFloat(parameterName, 1f);

        // 2. Set the visual slider to that position
        mySlider.value = savedVolume;

        // 3. Apply the volume to the game immediately
        SetVolume(savedVolume);

        // 4. Listen for future changes
        mySlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        // Convert linear slider (0 to 1) to Decibels (-80 to 0)
        // We use Log10 because human hearing is logarithmic
        float dbVolume = Mathf.Log10(sliderValue) * 20;

        // Apply to Mixer
        myMixer.SetFloat(parameterName, dbVolume);

        // Save for next time
        PlayerPrefs.SetFloat(parameterName, sliderValue);
        PlayerPrefs.Save();
    }
}