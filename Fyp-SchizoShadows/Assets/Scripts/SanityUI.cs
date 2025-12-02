using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    public SanityManager sanityManager; // drag SanityManager here
    public Slider sanitySlider;         // drag Slider here (min 0, max 1)

    void Start()
    {
        if (sanitySlider != null)
        {
            sanitySlider.minValue = 0f;
            sanitySlider.maxValue = 1f;
        }
    }

    void Update()
    {
        if (sanityManager == null || sanitySlider == null) return;
        sanitySlider.value = sanityManager.GetSanityPercent();
    }
}
