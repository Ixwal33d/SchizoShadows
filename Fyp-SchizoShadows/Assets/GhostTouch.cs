using UnityEngine;

public class GhostTouch : MonoBehaviour
{
    public GameManager gameManager; // Drag the GameOverCanvas here

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit the player (Make sure your Player object has the tag "Player")
        if (other.CompareTag("Player"))
        {
            gameManager.TriggerGameOver();
        }
    }
}