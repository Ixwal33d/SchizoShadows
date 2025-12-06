using UnityEngine;

public class SmartClueIndicator : MonoBehaviour
{
    public Transform playerCamera; // Drag your VR Camera (Main Camera) here
    public float hideDistance = 1.5f; // Hide arrow if player is closer than this
    public GameObject arrowVisuals; // The actual mesh of the arrow

    void Update()
    {
        // Calculate distance between arrow and player
        float distance = Vector3.Distance(transform.position, playerCamera.position);

        if (distance < hideDistance)
        {
            // Player is close, hide the arrow to clear up view
            arrowVisuals.SetActive(false);
        }
        else
        {
            // Player is far, show arrow so they can find the clue
            arrowVisuals.SetActive(true);

            // Add the bobbing/rotating logic here from the previous step
        }
    }
}