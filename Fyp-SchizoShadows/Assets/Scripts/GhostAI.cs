using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public enum GhostState
    {
        Idle,
        Wandering,
        Hunting,
        Returning
    }

    [Header("Current State (Read Only)")]
    public GhostState currentState = GhostState.Idle;

    [Header("References")]
    public SanityManager sanityManager;
    public Transform player;
    public Animator ghostAnimator;

    [Header("Wander Settings (No Patrol Points Needed!)")]
    public float wanderRadius = 20f;
    public float wanderSpeed = 2f;
    public float waitTimeAtDestination = 2f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    [Header("Detection Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleLayer;
    public bool canSeePlayer = false;

    [Header("Hunt Settings")]
    [Range(0f, 1f)]
    public float sanityThreshold = 0.5f;
    public float huntSpeed = 3f;
    public float stopDistance = 1.5f;
    public float losePlayerTime = 5f;
    public float huntStartDelay = 3f;
    private float losePlayerTimer = 0f;
    private float huntDelayTimer = 0f;
    private bool isHuntDelayActive = false;

    [Header("Visibility Settings")]
    public bool visibleDuringWander = false;
    public bool visibleDuringHunt = true;

    [Header("Audio")]
    public AudioSource wanderSound;
    public AudioSource huntingSound;
    public AudioSource catchSound;
    public AudioSource spottedSound;

    [Header("Catch Settings")]
    public bool canCatchPlayer = true;
    public float catchDistance = 1f;
    public int gameOverSceneIndex = 0;

    // Private variables
    private NavMeshAgent navAgent;
    private bool playerCaught = false;
    private Vector3 lastKnownPlayerPosition;
    private bool hasSeenPlayer = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Get NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("GhostAI: NavMeshAgent component required!");
            return;
        }

        // Auto-find SanityManager
        if (sanityManager == null)
        {
            sanityManager = FindObjectOfType<SanityManager>();
        }

        // Auto-find Player
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
        }

        // Get Animator
        if (ghostAnimator == null)
        {
            ghostAnimator = GetComponent<Animator>();
            if (ghostAnimator == null)
                ghostAnimator = GetComponentInChildren<Animator>();
        }

        // Start wandering
        SetState(GhostState.Wandering);
    }

    void Update()
    {
        if (playerCaught || player == null) return;

        // Always check if we can see the player
        canSeePlayer = CheckPlayerVisibility();

        // Check hunt triggers
        bool shouldHunt = ShouldStartHunting();

        // State machine
        switch (currentState)
        {
            case GhostState.Idle:
                HandleIdle();
                break;
            case GhostState.Wandering:
                HandleWander();
                break;
            case GhostState.Hunting:
                HandleHunt();
                break;
            case GhostState.Returning:
                HandleReturn();
                break;
        }

        // Check for state transitions
        if (currentState != GhostState.Hunting && shouldHunt)
        {
            StartHunting();
        }

        // Update animator
        UpdateAnimator();
    }

    #region State Handlers

    void HandleIdle()
    {
        navAgent.isStopped = true;
    }

    void HandleWander()
    {
        navAgent.speed = wanderSpeed;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                SetRandomDestination();
            }
            return;
        }

        // Check if reached destination
        if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            isWaiting = true;
            waitTimer = waitTimeAtDestination;
        }
    }

    void HandleHunt()
    {
        // Handle hunt delay at the start
        if (isHuntDelayActive)
        {
            huntDelayTimer -= Time.deltaTime;
            navAgent.isStopped = true;

            if (huntDelayTimer <= 0)
            {
                isHuntDelayActive = false;
                navAgent.isStopped = false;
                Debug.Log("Ghost: Hunt delay over, now chasing!");
            }
            return;
        }

        navAgent.speed = huntSpeed;

        // Check catch distance
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (canCatchPlayer && distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (canSeePlayer)
        {
            // We can see the player - chase them!
            lastKnownPlayerPosition = player.position;
            navAgent.SetDestination(player.position);
            losePlayerTimer = losePlayerTime;
            hasSeenPlayer = true;
        }
        else if (hasSeenPlayer)
        {
            // Lost sight - go to last known position
            navAgent.SetDestination(lastKnownPlayerPosition);

            losePlayerTimer -= Time.deltaTime;

            // Check if we reached last known position
            if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
            {
                losePlayerTimer -= Time.deltaTime * 2;
            }

            // Lost the player completely
            if (losePlayerTimer <= 0)
            {
                if (sanityManager != null && sanityManager.GetSanityPercent() <= sanityThreshold)
                {
                    losePlayerTimer = losePlayerTime;
                }
                else
                {
                    StopHunting();
                }
            }
        }
        else
        {
            // Hunting due to low sanity but haven't seen player yet
            navAgent.SetDestination(player.position);
        }
    }

    void HandleReturn()
    {
        navAgent.speed = wanderSpeed;

        if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            SetState(GhostState.Wandering);
        }
    }

    #endregion

    #region Random Wander

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
            Debug.Log($"Ghost: Wandering to new position");
        }
        else
        {
            // Couldn't find valid position, try again
            SetRandomDestination();
        }
    }

    #endregion

    #region Detection

    bool CheckPlayerVisibility()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within view distance
        if (distanceToPlayer > viewDistance) return false;

        // Check if player is within view angle
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // Raycast to check for obstacles
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 playerCenter = player.position + Vector3.up * 1f;

        RaycastHit hit;
        if (Physics.Raycast(eyePosition, (playerCenter - eyePosition).normalized, out hit, viewDistance, obstacleLayer))
        {
            if (hit.transform != player && !hit.transform.IsChildOf(player))
            {
                return false;
            }
        }

        return true;
    }

    bool ShouldStartHunting()
    {
        if (canSeePlayer) return true;

        if (sanityManager != null && sanityManager.GetSanityPercent() <= sanityThreshold)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region State Transitions

    void SetState(GhostState newState)
    {
        currentState = newState;
        Debug.Log($"Ghost State: {newState}");

        switch (newState)
        {
            case GhostState.Idle:
                navAgent.isStopped = true;
                SetVisibility(visibleDuringWander);
                break;

            case GhostState.Wandering:
                navAgent.isStopped = false;
                SetVisibility(visibleDuringWander);
                SetRandomDestination();
                if (wanderSound != null) wanderSound.Play();
                break;

            case GhostState.Hunting:
                navAgent.isStopped = false;
                SetVisibility(visibleDuringHunt);
                break;

            case GhostState.Returning:
                navAgent.isStopped = false;
                SetVisibility(visibleDuringWander);
                SetRandomDestination();
                break;
        }
    }

    void StartHunting()
    {
        Debug.Log("Ghost: HUNTING STARTED!");

        SetState(GhostState.Hunting);

        // Start the hunt delay
        huntDelayTimer = huntStartDelay;
        isHuntDelayActive = true;

        losePlayerTimer = losePlayerTime;
        hasSeenPlayer = canSeePlayer;

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
        }

        if (spottedSound != null)
        {
            spottedSound.Play();
        }

        if (huntingSound != null && !huntingSound.isPlaying)
        {
            huntingSound.Play();
        }

        if (wanderSound != null)
        {
            wanderSound.Stop();
        }
    }

    void StopHunting()
    {
        Debug.Log("Ghost: Lost the player, returning to wander");

        hasSeenPlayer = false;

        if (huntingSound != null)
        {
            huntingSound.Stop();
        }

        SetState(GhostState.Returning);
    }

    #endregion

    #region Visibility

    void SetVisibility(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }

    #endregion

    #region Catch Player

    void CatchPlayer()
    {
        playerCaught = true;
        Debug.Log("Ghost: PLAYER CAUGHT!");

        navAgent.isStopped = true;

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

    #endregion

    #region Animator

    void UpdateAnimator()
    {
        if (ghostAnimator == null) return;

        bool isMoving = navAgent.velocity.magnitude > 0.1f;
        bool isHunting = currentState == GhostState.Hunting;

        ghostAnimator.SetBool("isHunting", isHunting);
        ghostAnimator.SetBool("isMoving", isMoving);
        ghostAnimator.SetFloat("speed", navAgent.velocity.magnitude);
    }

    #endregion

    #region Public Methods

    public void ForceHunt()
    {
        StartHunting();
    }

    public void ResetGhost()
    {
        playerCaught = false;
        hasSeenPlayer = false;
        transform.position = startPosition;
        SetState(GhostState.Wandering);
    }

    #endregion

    #region Debug Visualization

    void OnDrawGizmosSelected()
    {
        // Draw wander radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Draw view distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Draw view cone
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Draw catch distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        // Draw line to player if can see
        if (canSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

    #endregion
}