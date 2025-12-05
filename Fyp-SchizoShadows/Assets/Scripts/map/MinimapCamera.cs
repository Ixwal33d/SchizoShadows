using UnityEngine;

/// <summary>
/// Minimap Camera Controller
/// Shows top-down view of the game world
/// </summary>
public class MinimapCamera : MonoBehaviour
{
    [Header("Minimap Settings")]
    [SerializeField] private Transform player; // Your VR camera/player
    [SerializeField] private float height = 20f; // How high above player
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private bool rotateWithPlayer = false;

    [Header("Camera Settings")]
    [SerializeField] private float cameraSize = 10f; // Zoom level
    [SerializeField] private LayerMask minimapLayers = -1; // What to show

    private Camera minimapCam;

    void Start()
    {
        SetupMinimapCamera();
    }

    void SetupMinimapCamera()
    {
        // Get or add camera
        minimapCam = GetComponent<Camera>();
        if (minimapCam == null)
        {
            minimapCam = gameObject.AddComponent<Camera>();
        }

        // Configure camera for minimap
        minimapCam.orthographic = true; // Top-down view
        minimapCam.orthographicSize = cameraSize;
        minimapCam.cullingMask = minimapLayers;
        minimapCam.depth = 1; // Render on top
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = Color.black;

        // Position camera
        transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Look down

        Debug.Log("✅ Minimap camera setup complete!");

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            if (cam != null)
            {
                player = cam.transform;
                Debug.Log($"✅ Auto-found player: {player.name}");
            }
        }
    }

    void LateUpdate()
    {
        if (followPlayer && player != null)
        {
            // Follow player position (but stay above)
            Vector3 newPosition = player.position;
            newPosition.y += height;
            transform.position = newPosition;

            // Optionally rotate with player
            if (rotateWithPlayer)
            {
                Vector3 rotation = transform.rotation.eulerAngles;
                rotation.y = player.rotation.eulerAngles.y;
                transform.rotation = Quaternion.Euler(rotation);
            }
        }
    }

    // Public methods to control minimap
    public void SetZoom(float size)
    {
        cameraSize = size;
        if (minimapCam != null)
        {
            minimapCam.orthographicSize = size;
        }
    }

    public void SetHeight(float h)
    {
        height = h;
    }

    public void ToggleFollow(bool follow)
    {
        followPlayer = follow;
    }
}
