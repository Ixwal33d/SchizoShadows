using UnityEngine;

public class SubtleGlowKey : MonoBehaviour
{
    [Header("Subtle Glow Settings")]
    [SerializeField] private Color glowColor = new Color(0.9f, 0.9f, 1f, 1f);
    [SerializeField] private float baseEmissionIntensity = 0.3f;
    [SerializeField] private float pulseIntensity = 0.5f;
    [SerializeField] private float pulseSpeed = 1.5f;

    [Header("Optional Light")]
    [SerializeField] private bool addPointLight = false;
    [SerializeField] private float lightIntensity = 0.5f;
    [SerializeField] private float lightRange = 2f;

    [Header("Rotation (Optional)")]
    [SerializeField] private bool rotateKey = false;
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
            // Create a new material instance so we don't change the shared asset
            keyMaterial = new Material(keyRenderer.sharedMaterial);
            keyRenderer.material = keyMaterial;

            keyMaterial.EnableKeyword("_EMISSION");
            keyMaterial.SetColor("_EmissionColor", glowColor * baseEmissionIntensity);
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
        // Subtle pulsing effect
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

    // Called by EscapeRoomKey.cs on pickup
    public void DisableGlow()
    {
        if (keyMaterial != null)
        {
            keyMaterial.SetColor("_EmissionColor", Color.black);
            keyMaterial.DisableKeyword("_EMISSION");
        }

        if (pointLight != null)
        {
            // Clean up the dynamically created light object
            Destroy(pointLight.gameObject);
        }

        enabled = false; // Stop the Update() loop from running
        Debug.Log("Glow effect permanently disabled.");
    }
}