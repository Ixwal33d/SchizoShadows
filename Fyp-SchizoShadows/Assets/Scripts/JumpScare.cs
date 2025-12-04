using UnityEngine;

public class JumpScare : MonoBehaviour
{
    [Header("References")]
    public Transform player;           // The VR player/camera
    public GameObject scareObject;     // The scary head
    public AudioSource scareSound;     // Scary sound effect
    
    [Header("Settings")]
    public float moveSpeed = 5f;       // How fast the head moves toward player
    public float stopDistance = 0.5f;  // How close it gets before stopping
    public float scareDelay = 0.5f;    // Delay after door opens before scare starts
    
    private bool isScaring = false;
    private bool hasScared = false;    // Only scare once
    private Vector3 startPosition;
    
    void Start()
    {
        // Save the starting position
        if (scareObject != null)
        {
            startPosition = scareObject.transform.position;
            scareObject.SetActive(false);  // Hide at start
        }
    }
    
    void Update()
    {
        if (isScaring && scareObject != null && player != null)
        {
            // Move toward the player's head
            Vector3 direction = (player.position - scareObject.transform.position).normalized;
            float distance = Vector3.Distance(scareObject.transform.position, player.position);
            
            // Keep moving until we reach stop distance
            if (distance > stopDistance)
            {
                scareObject.transform.position += direction * moveSpeed * Time.deltaTime;
                
                // Make the head look at the player
                scareObject.transform.LookAt(player);
            }
            else
            {
                // Stop and optionally disappear after a moment
                StartCoroutine(DisappearAfterDelay(2f));
            }
        }
    }
    
    // Call this function when the wardrobe door opens
    public void TriggerJumpScare()
    {
        if (!hasScared)
        {
            hasScared = true;
            StartCoroutine(StartScareAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator StartScareAfterDelay()
    {
        yield return new WaitForSeconds(scareDelay);
        
        // Show the scary object
        if (scareObject != null)
        {
            scareObject.SetActive(true);
        }
        
        // Play the scary sound
        if (scareSound != null)
        {
            scareSound.Play();
        }
        
        isScaring = true;
    }
    
    private System.Collections.IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isScaring = false;
        
        if (scareObject != null)
        {
            scareObject.SetActive(false);
        }
    }
    
    // Call this to reset the jump scare (if you want it to work again)
    public void ResetJumpScare()
    {
        hasScared = false;
        isScaring = false;
        
        if (scareObject != null)
        {
            scareObject.transform.position = startPosition;
            scareObject.SetActive(false);
        }
    }
}