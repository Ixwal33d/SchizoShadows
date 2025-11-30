using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    public SanityManager sanitySystem;
    public Slider sanitySlider;

    void Update()
    {
        if (sanitySystem != null)
        {
            sanitySlider.value = sanitySystem.GetSanityPercent();
        }
    }
}

