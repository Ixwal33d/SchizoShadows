using UnityEngine;

public class GhostHunter : MonoBehaviour
{
    [Header("References")]
    public SanityManager sanityManager;      // Reference to sanity manager
    public Transform player;                  // The XR Origin / Player
    public Animator ghostAnimator;            // Ghost's animator component

    [Header("Hunt Settings")]
    [Range(0f, 1f)]
    public float sanityThreshold = 0.5f;      // Start hunting at 50% sanity
    public float huntSpeed = 3f;              // How fast ghost moves when hunting
    public float stopDistance = 1.5f;         // How close ghost gets before stopping
    public float rotationSpeed = 5f;          // How fast ghost turns toward player

    [Header("Spawn Settings")]
    public Transform spawnPoint;              // Where ghost spawns/starts from
    public float returnSpeed = 2f;            // Speed when returning to spawn

    [Header("Audio (Optional)")]
    public AudioSource huntingSound;          // Scary ambient sound when hunting
    public AudioSource catchSound;            // Sound when ghost catches player

    [Header("Catch Settings")]
    public bool canCatchPlayer = true;        // Can the ghost catch the player?
    public float catchDistance = 1f;          // Distance to "catch" player
    public int gameOverSceneIndex = 0;        // Scene to load on catch (game over)

    // Private variables
    private bool isHunting = false;
    private bool hasSpawnPoint = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool playerCaught = false;

    void Start()
    {
        // Store original position if no spawn point assigned
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (spawnPoint != null)
        {
            hasSpawnPoint = true;
            originalPosition = spawnPoint.position;
            originalRotation = spawnPoint.rotation;
        }

        // Auto-find references if not assigned
        if (sanityManager == null)
        {
            sanityManager = FindObjectOfType<SanityManager>();
            if (sanityManager == null)
                Debug.LogError("GhostHunter: No SanityManager found!");
        }

        if (player == null)
        {
            // Try to find XR Origin
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                player = xrOrigin.transform;
            }
            else
            {
                // Fallback: find main camera
                player = Camera.main?.transform;
            }

            if (player == null)
                Debug.LogError("GhostHunter: No player/XR Origin found!");
        }

        if (ghostAnimator == null)
        {
            ghostAnimator = GetComponent<Animator>();
            if (ghostAnimator == null)
                ghostAnimator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (sanityManager == null || player == null || playerCaught) return;

        // Check if sanity is below threshold
        float sanityPercent = sanityManager.GetSanityPercent();
        bool shouldHunt = sanityPercent <= sanityThreshold;

        // State changed - start or stop hunting
        if (shouldHunt && !isHunting)
        {
            StartHunting();
        }
        else if (!shouldHunt && isHunting)
        {
            StopHunting();
        }

        // Execute behavior based on state
        if (isHunting)
        {
            HuntPlayer();
        }
        else
        {
            ReturnToSpawn();
        }
    }

    void StartHunting()
    {
        isHunting = true;
        Debug.Log("Ghost: HUNTING STARTED - Sanity too low!");

        // Trigger animation
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", true);
        }

        // Play hunting sound
        if (huntingSound != null && !huntingSound.isPlaying)
        {
            huntingSound.Play();
        }
    }

    void StopHunting()
    {
        isHunting = false;
        Debug.Log("Ghost: Stopped hunting - Sanity restored");

        // Trigger animation
        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", false);
        }

        // Stop hunting sound
        if (huntingSound != null && huntingSound.isPlaying)
        {
            huntingSound.Stop();
        }
    }

    void HuntPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if caught player
        if (canCatchPlayer && distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        // Move toward player if not too close
        if (distanceToPlayer > stopDistance)
        {
            // Calculate direction to player (ignore Y for ground movement, or include for flying ghost)
            Vector3 direction = (player.position - transform.position).normalized;

            // For ground movement, zero out Y
            // direction.y = 0; // Uncomment this if ghost should stay on ground

            // Move toward player
            transform.position += direction * huntSpeed * Time.deltaTime;

            // Rotate to face player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void ReturnToSpawn()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, originalPosition);

        // If not at spawn point, move back
        if (distanceToSpawn > 0.5f)
        {
            Vector3 direction = (originalPosition - transform.position).normalized;
            transform.position += direction * returnSpeed * Time.deltaTime;

            // Rotate back to original rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void CatchPlayer()
    {
        playerCaught = true;
        Debug.Log("Ghost: PLAYER CAUGHT!");

        // Play catch sound
        if (catchSound != null)
        {
            catchSound.Play();
        }

        // Stop hunting sound
        if (huntingSound != null)
        {
            huntingSound.Stop();
        }

        // Trigger game over (load scene)
        if (SceneTransitionManager.singleton != null)
        {
            SceneTransitionManager.singleton.GoToScene(gameOverSceneIndex);
        }
        else
        {
            // Fallback: load scene directly
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneIndex);
        }
    }

    // Public methods to control the ghost externally
    public void ForceStartHunting()
    {
        StartHunting();
    }

    public void ForceStopHunting()
    {
        StopHunting();
    }

    public void ResetGhost()
    {
        playerCaught = false;
        isHunting = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", false);
        }
    }

    // Visualize in editor
    void OnDrawGizmosSelected()
    {
        // Show hunt range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Show catch range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        // Show spawn point
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
    }
}