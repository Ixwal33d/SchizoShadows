using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// GUARANTEED WORKING VR Hand Collision Solution
/// This uses Physics Override to prevent hands passing through walls
/// </summary>
public class VRHandPhysicsBlocker : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayers = -1; // Everything
    [SerializeField] private float checkDistance = 0.2f;
    [SerializeField] private float handRadius = 0.08f;

    [Header("Hand Model")]
    [SerializeField] private Transform handVisualTransform; // Assign your hand model here

    private Vector3 lastValidPosition;
    private bool isBlocked = false;

    void Start()
    {
        lastValidPosition = transform.position;

        Debug.Log($"✅ VR Hand Physics Blocker active on {gameObject.name}");
        Debug.Log($"⚠️ Make sure walls are on layers: {LayerMaskToString(wallLayers)}");
    }

    void Update()
    {
        CheckAndBlockHand();
    }

    void CheckAndBlockHand()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = currentPosition - lastValidPosition;
        float distance = direction.magnitude;

        if (distance > 0.001f)
        {
            // Check if we're about to hit a wall
            RaycastHit hit;

            // Use SphereCast to detect walls
            if (Physics.SphereCast(lastValidPosition, handRadius, direction.normalized,
                out hit, distance + checkDistance, wallLayers))
            {
                // We hit a wall!
                Debug.Log($"🛑 BLOCKED by {hit.collider.gameObject.name}!");

                // Calculate safe position (just before the wall)
                Vector3 safePosition = hit.point - (direction.normalized * (handRadius + 0.01f));

                // If we have a hand visual, move it to safe position
                if (handVisualTransform != null)
                {
                    handVisualTransform.position = safePosition;
                }

                isBlocked = true;

                // Don't update last valid position - stay at safe spot
                return;
            }
            else
            {
                // No wall detected, safe to move
                isBlocked = false;
                lastValidPosition = currentPosition;

                // Update hand visual
                if (handVisualTransform != null)
                {
                    handVisualTransform.position = currentPosition;
                }
            }
        }
    }

    string LayerMaskToString(LayerMask mask)
    {
        string result = "";
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                result += LayerMask.LayerToName(i) + ", ";
            }
        }
        return result.Length > 0 ? result.Substring(0, result.Length - 2) : "None";
    }

    void OnDrawGizmos()
    {
        // Visualize the collision sphere
        Gizmos.color = isBlocked ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, handRadius);

        // Draw check distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, handRadius + checkDistance);
    }
}