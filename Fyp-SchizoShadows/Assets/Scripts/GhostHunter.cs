using UnityEngine;
using UnityEngine.AI;

public class GhostHunter : MonoBehaviour
{
    [Header("References")]
    public SanityManager sanityManager;
    public Transform player;
    public Animator ghostAnimator;

    [Header("Hunt Settings")]
    [Range(0f, 1f)]
    public float sanityThreshold = 0.5f;
    public float huntSpeed = 3f;
    public float stopDistance = 1.5f;
    public float rotationSpeed = 5f;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float returnSpeed = 2f;

    [Header("Audio (Optional)")]
    public AudioSource huntingSound;
    public AudioSource catchSound;

    [Header("Catch Settings")]
    public bool canCatchPlayer = true;
    public float catchDistance = 1f;
    public int gameOverSceneIndex = 0;

    // Private variables
    private bool isHunting = false;
    private bool hasSpawnPoint = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool playerCaught = false;
    private NavMeshAgent navAgent;
    private bool useNavMesh = false;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (spawnPoint != null)
        {
            hasSpawnPoint = true;
            originalPosition = spawnPoint.position;
            originalRotation = spawnPoint.rotation;
        }

        // Check for NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            useNavMesh = true;
            navAgent.speed = huntSpeed;
            navAgent.stoppingDistance = stopDistance;
            navAgent.isStopped = true;
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
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                player = xrOrigin.transform;
            }
            else
            {
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

        float sanityPercent = sanityManager.GetSanityPercent();
        bool shouldHunt = sanityPercent <= sanityThreshold;

        if (shouldHunt && !isHunting)
        {
            StartHunting();
        }
        else if (!shouldHunt && isHunting)
        {
            StopHunting();
        }

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

        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", true);
        }

        if (useNavMesh && navAgent != null)
        {
            navAgent.isStopped = false;
        }

        if (huntingSound != null && !huntingSound.isPlaying)
        {
            huntingSound.Play();
        }
    }

    void StopHunting()
    {
        isHunting = false;
        Debug.Log("Ghost: Stopped hunting - Sanity restored");

        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", false);
        }

        if (useNavMesh && navAgent != null)
        {
            navAgent.isStopped = true;
        }

        if (huntingSound != null && huntingSound.isPlaying)
        {
            huntingSound.Stop();
        }
    }

    void HuntPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (canCatchPlayer && distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (useNavMesh && navAgent != null)
        {
            // Use NavMesh pathfinding
            navAgent.SetDestination(player.position);
        }
        else
        {
            // Fallback: Simple movement (keeps original height)
            if (distanceToPlayer > stopDistance)
            {
                Vector3 targetPosition = player.position;
                targetPosition.y = originalPosition.y;

                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * huntSpeed * Time.deltaTime;

                if (direction != Vector3.zero)
                {
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }
            }
        }
    }

    void ReturnToSpawn()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, originalPosition);

        if (distanceToSpawn > 0.5f)
        {
            if (useNavMesh && navAgent != null)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(originalPosition);
            }
            else
            {
                Vector3 direction = (originalPosition - transform.position).normalized;
                transform.position += direction * returnSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (useNavMesh && navAgent != null)
            {
                navAgent.isStopped = true;
            }
        }
    }

    void CatchPlayer()
    {
        playerCaught = true;
        Debug.Log("Ghost: PLAYER CAUGHT!");

        if (useNavMesh && navAgent != null)
        {
            navAgent.isStopped = true;
        }

        if (catchSound != null)
        {
            catchSound.Play();
        }

        if (huntingSound != null)
        {
            huntingSound.Stop();
        }

        if (SceneTransitionManager.singleton != null)
        {
            SceneTransitionManager.singleton.GoToScene(gameOverSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneIndex);
        }
    }

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

        if (useNavMesh && navAgent != null)
        {
            navAgent.isStopped = true;
        }

        if (ghostAnimator != null)
        {
            ghostAnimator.SetBool("isHunting", false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
    }
}