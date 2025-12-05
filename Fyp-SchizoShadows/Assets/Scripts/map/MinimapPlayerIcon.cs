using UnityEngine;

/// <summary>
/// Player Icon for Minimap
/// Shows where player is on the minimap
/// </summary>
public class MinimapPlayerIcon : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private Transform player; // Your VR camera
    [SerializeField] private float heightOffset = 10f; // How high to float
    [SerializeField] private bool rotateWithPlayer = true;

    [Header("Visual Settings")]
    [SerializeField] private Color iconColor = Color.blue;
    [SerializeField] private float iconSize = 1f;

    private SpriteRenderer iconRenderer;

    void Start()
    {
        SetupPlayerIcon();
    }

    void SetupPlayerIcon()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            if (cam != null)
            {
                player = cam.transform;
                Debug.Log($"✅ Minimap icon following: {player.name}");
            }
        }

        // Create sprite renderer for icon
        iconRenderer = GetComponent<SpriteRenderer>();
        if (iconRenderer == null)
        {
            iconRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Create simple triangle sprite (or you can assign custom sprite)
        iconRenderer.color = iconColor;
        iconRenderer.sortingOrder = 100; // Render on top

        // Scale the icon
        transform.localScale = Vector3.one * iconSize;

        Debug.Log("✅ Minimap player icon setup!");
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Follow player position (float above)
            Vector3 newPosition = player.position;
            newPosition.y += heightOffset;
            transform.position = newPosition;

            // Rotate to show player direction
            if (rotateWithPlayer)
            {
                Vector3 rotation = player.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(90f, rotation.y, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }
}