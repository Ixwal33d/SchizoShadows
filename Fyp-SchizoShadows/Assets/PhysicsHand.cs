using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    [Header("Target")]
    public Transform targetController; // Drag your XR Controller (Right Hand) here

    [Header("Settings")]
    public float moveSpeed = 50f; // How fast the hand snaps to the controller

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Detach this hand from the player so it moves freely
        transform.parent = null;
    }

    void FixedUpdate()
    {
        // 1. Calculate direction to the controller
        Vector3 direction = targetController.position - transform.position;

        // 2. Move using Velocity (Physics) instead of Position (Teleport)
        // This allows the Bed Collider to stop us!
        rb.velocity = direction * moveSpeed;

        // 3. Match Rotation (Optional)
        rb.MoveRotation(targetController.rotation);
    }
}