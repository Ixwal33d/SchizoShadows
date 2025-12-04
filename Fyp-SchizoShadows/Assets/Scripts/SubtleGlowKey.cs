using UnityEngine;

public class SubtleGlowKey : MonoBehaviour
{
    [Header("Subtle Glow Settings")]
    [SerializeField] private Color glowColor = new Color(0.9f, 0.9f, 1f, 1f); // Soft white-blue
    [SerializeField] private float baseEmissionIntensity = 0.3f; // Very subtle
    [SerializeField] private float pulseIntensity = 0.5f; // Max intensity during pulse
    [SerializeField] private float pulseSpeed = 1.5f; // Slow, subtle pulse

    [Header("Optional Light")]
    [SerializeField] private bool addPointLight = false; // Usually not needed for subtle effect
    [SerializeField] private float lightIntensity = 0.5f;
    [SerializeField] private float lightRange = 2f;

    [Header("Rotation (Optional)")]
    [SerializeField] private bool rotateKey = false; // Usually off for realistic look
    [SerializeField] private float rotationSpeed = 20f;

    private Material keyMaterial;
    private Light pointLight;
    private Renderer keyRenderer;

    void Start()
    {
        SetupSubtleGlow();

        if (addPointLight)
        {
            SetupPointLight();
        }
    }

    void SetupSubtleGlow()
    {
        keyRenderer = GetComponent<Renderer>();

        if (keyRenderer != null)
        {
            // Create a new material instance
            keyMaterial = new Material(keyRenderer.sharedMaterial);
            keyRenderer.material = keyMaterial;

            // Enable emission
            keyMaterial.EnableKeyword("_EMISSION");

            // Set initial subtle glow
            keyMaterial.SetColor("_EmissionColor", glowColor * baseEmissionIntensity);

            Debug.Log("✓ Subtle glow effect applied to " + gameObject.name);
        }
        else
        {
            Debug.LogError("❌ No Renderer found on " + gameObject.name + "! Add a Mesh Renderer.");
        }
    }

    void SetupPointLight()
    {
        GameObject lightObj = new GameObject("Key Light");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = glowColor;
        pointLight.intensity = lightIntensity;
        pointLight.range = lightRange;
        pointLight.shadows = LightShadows.None;
    }

    void Update()
    {
        // Very subtle pulsing effect
        float pulse = Mathf.Lerp(baseEmissionIntensity, pulseIntensity,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        if (keyMaterial != null)
        {
            keyMaterial.SetColor("_EmissionColor", glowColor * pulse);
        }

        if (pointLight != null)
        {
            pointLight.intensity = pulse * lightIntensity;
        }

        // Optional rotation
        if (rotateKey)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    // Call this when key is picked up
    public void DisableGlow()
    {
        if (keyMaterial != null)
        {
            keyMaterial.SetColor("_EmissionColor", Color.black);
            keyMaterial.DisableKeyword("_EMISSION");
        }

        if (pointLight != null)
        {
            Destroy(pointLight.gameObject);
        }

        enabled = false;
    }

    // Optional: Add outline shader effect for even more visibility
    // This would require a separate outline shader
}
