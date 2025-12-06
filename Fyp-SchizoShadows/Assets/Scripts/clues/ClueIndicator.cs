using UnityEngine;

public class ClueIndicator : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How high and low the arrow moves")]
    public float bobAmplitude = 0.1f;

    [Tooltip("How fast the arrow moves up and down")]
    public float bobFrequency = 2f;

    [Header("Rotation Settings")]
    public float rotateSpeed = 50f;

    // To store the original position
    private Vector3 startPos;

    void Start()
    {
        // Remember where we placed the arrow initially
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. Handle the Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. Handle the Bobbing (Sine Wave)
        // We use Math.Sin to create a smooth wave between -1 and 1
        float newY = startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;

        // Apply the new position
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}