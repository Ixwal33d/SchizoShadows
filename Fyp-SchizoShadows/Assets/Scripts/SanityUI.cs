using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    public SanityManager sanitySystem;
    public Slider sanitySlider;

    void Start()
    {
        sanitySlider.minValue = 0f;
        sanitySlider.maxValue = 1f;
    }

    void Update()
    {
        sanitySlider.value = sanitySystem.GetSanityPercent();
    }
}
