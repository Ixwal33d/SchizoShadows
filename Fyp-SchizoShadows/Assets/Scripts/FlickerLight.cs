using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    private Light lightSource;

    void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        lightSource.intensity = Random.Range(0.3f, 0.7f);
    }
}
