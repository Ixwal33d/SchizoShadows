using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PhasmoGhost : MonoBehaviour
{
    public enum GhostState
    {
        Idle,
        Wandering,
        HuntWarning,    // Lights flicker, warning before hunt
        Hunting,
        Searching,
        Cooldown        // Rest period after hunt ends
    }

    [Header("=== CURRENT STATE (Read Only) ===")]
    public GhostState currentState = GhostState.Idle;
    public float currentHuntTimer = 0f;

    [Header("=== REFERENCES ===")]
    public SanityManager sanityManager;
    public Transform player;
    public Animator ghostAnimator;

    [Header("=== WANDER SETTINGS ===")]
    public float wanderRadius = 30f;
    public float wanderSpeed = 2f;
    public float waitTimeAtDestination = 3f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    [Header("=== HUNT TRIGGER SETTINGS ===")]
    [Range(0f, 1f)]
    public float sanityThreshold = 0.5f;
    public float huntCheckInterval = 30f;       // Check for hunt every X seconds
    public float huntChanceMultiplier = 1f;     // Higher = more frequent hunts
    private float huntCheckTimer = 0f;

    [Header("=== HUNT SETTINGS ===")]
    public float huntDuration = 30f;            // Hunt lasts 30 seconds
    public float huntSpeed = 5f;
    public float huntWarningDuration = 3f;      // Warning before hunt starts
    public float searchDuration = 10f;          // Time spent searching after losing player
    public float huntCooldown = 20f;            // Time before ghost can hunt again
    public float catchDistance = 1.2f;
    private float cooldownTimer = 0f;

    [Header("=== DETECTION SETTINGS ===")]
    public float viewDistance = 12f;
    public float viewAngle = 110f;
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;

    [Header("=== LIGHT FLICKER SETTINGS ===")]
    public List<Light> allLights = new List<Light>();
    public float flickerSpeed = 0.1f;
    public bool autoFindLights = true;



    [Header("=== AUDIO ===")]
    public AudioSource heartbeatSound;          // Plays during hunt warning
    public AudioSource huntingMusic;            // Intense music during hunt
    public AudioSource ghostBreathing;          // Ambient ghost sounds
    public AudioSource catchSound;              // When ghost catches player

    [Header("=== DEBUG / TESTING ===")]
    public bool alwaysVisible = false;          // Keep ghost visible for testing

    [Header("=== JUMPSCARE SETTINGS ===")]
    public float jumpscareDistance = 0.5f;      // How close ghost face gets to player
    public float jumpscareDuration = 2f;        // How long jumpscare lasts
    public float shakeIntensity = 0.3f;         // Screen shake strength
    public float shakeSpeed = 30f;              // Screen shake speed
    public AudioSource jumpscareSound;          // Loud scream sound
    public GameObject jumpscareImage;           // Optional: 2D scary face image on screen

    [Header("=== SCREEN EFFECTS ===")]
    public GameObject huntWarningEffect;        // Red vignette or screen effect
    public GameObject huntActiveEffect;         // Effect during active hunt

    [Header("=== GAME OVER ===")]
    public int gameOverSceneIndex = 0;

    // Private variables
    private NavMeshAgent navAgent;
    private bool playerCaught = false;
    private float huntTimer = 0f;
    private float searchTimer = 0f;
    private float warningTimer = 0f;
    private Coroutine flickerCoroutine;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("PhasmoGhost: NavMeshAgent required!");
            return;
        }

        // Auto-find sanity manager
        if (sanityManager == null)
            sanityManager = FindObjectOfType<SanityManager>();

        // Auto-find player
        if (player == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
                player = xrOrigin.transform;
            else
                player = Camera.main?.transform;
        }

        // Auto-find animator
        if (ghostAnimator == null)
        {
            ghostAnimator = GetComponent<Animator>();
            if (ghostAnimator == null)
                ghostAnimator = GetComponentInChildren<Animator>();
        }

        // Auto-find all lights in scene
        if (autoFindLights)
        {
            allLights.AddRange(FindObjectsOfType<Light>());
        }

        // Disable screen effects
        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);
        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);

        // Start wandering (invisible)
        SetState(GhostState.Wandering);
        SetGhostVisibility(false);
    }

    void Update()
    {
        if (playerCaught || player == null) return;

        // Update visibility check
        canSeePlayer = CheckPlayerVisibility();

        // State machine
        switch (currentState)
        {
            case GhostState.Idle:
                HandleIdle();
                break;
            case GhostState.Wandering:
                HandleWander();
                CheckForHuntTrigger();
                break;
            case GhostState.HuntWarning:
                HandleHuntWarning();
                break;
            case GhostState.Hunting:
                HandleHunting();
                break;
            case GhostState.Searching:
                HandleSearching();
                break;
            case GhostState.Cooldown:
                HandleCooldown();
                break;
        }

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

        if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            isWaiting = true;
            waitTimer = waitTimeAtDestination;
        }
    }

    void CheckForHuntTrigger()
    {
        if (sanityManager == null) return;

        huntCheckTimer += Time.deltaTime;

        if (huntCheckTimer >= huntCheckInterval)
        {
            huntCheckTimer = 0f;

            float sanityPercent = sanityManager.GetSanityPercent();

            // Lower sanity = higher hunt chance
            if (sanityPercent <= sanityThreshold)
            {
                // Calculate hunt chance based on sanity
                float huntChance = (1f - sanityPercent) * huntChanceMultiplier;
                float roll = Random.Range(0f, 1f);

                Debug.Log($"Hunt check: Sanity {sanityPercent:P0}, Chance {huntChance:P0}, Roll {roll:F2}");

                if (roll <= huntChance)
                {
                    StartHuntWarning();
                }
            }
        }

        // Also start hunt if ghost sees player directly
        if (canSeePlayer && currentState == GhostState.Wandering)
        {
            StartHuntWarning();
        }
    }

    void HandleHuntWarning()
    {
        navAgent.isStopped = true;
        warningTimer -= Time.deltaTime;

        if (warningTimer <= 0)
        {
            StartHunt();
        }
    }

    void HandleHunting()
    {
        navAgent.speed = huntSpeed;
        huntTimer -= Time.deltaTime;
        currentHuntTimer = huntTimer;

        // Check if hunt time is over
        if (huntTimer <= 0)
        {
            EndHunt();
            return;
        }

        // Check for catch
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (canSeePlayer)
        {
            // Chase player
            lastKnownPlayerPosition = player.position;
            navAgent.SetDestination(player.position);
        }
        else
        {
            // Lost sight, go to last known position
            navAgent.SetDestination(lastKnownPlayerPosition);

            // If reached last known position, start searching
            if (!navAgent.pathPending && navAgent.remainingDistance < 2f)
            {
                SetState(GhostState.Searching);
                searchTimer = searchDuration;
            }
        }
    }

    void HandleSearching()
    {
        navAgent.speed = wanderSpeed;
        searchTimer -= Time.deltaTime;

        // Search random nearby locations
        if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            SearchNearbyLocation();
        }

        // Check if we can see player while searching
        if (canSeePlayer)
        {
            SetState(GhostState.Hunting);
            huntTimer = huntDuration * 0.5f; // Resume hunt with half time
            return;
        }

        // Search time over
        if (searchTimer <= 0)
        {
            EndHunt();
        }
    }

    void HandleCooldown()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            SetState(GhostState.Wandering);
        }
    }

    #endregion

    #region Hunt Control

    void StartHuntWarning()
    {
        Debug.Log("Ghost: HUNT WARNING - Lights flickering!");
        SetState(GhostState.HuntWarning);
        warningTimer = huntWarningDuration;

        // Start light flickering
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerLights());

        // Play heartbeat
        if (heartbeatSound != null)
        {
            heartbeatSound.Play();
        }

        // Show warning effect
        if (huntWarningEffect != null)
        {
            huntWarningEffect.SetActive(true);
        }
    }

    void StartHunt()
    {
        Debug.Log("Ghost: HUNT STARTED!");
        SetState(GhostState.Hunting);
        huntTimer = huntDuration;
        currentHuntTimer = huntTimer;

        // Make ghost visible
        SetGhostVisibility(true);

        // Set destination to player
        lastKnownPlayerPosition = player.position;
        navAgent.SetDestination(player.position);
        navAgent.isStopped = false;

        // Switch warning to hunt effect
        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);
        if (huntActiveEffect != null) huntActiveEffect.SetActive(true);

        // Play hunt music
        if (huntingMusic != null && !huntingMusic.isPlaying)
        {
            huntingMusic.Play();
        }

        // Stop heartbeat
        if (heartbeatSound != null)
        {
            heartbeatSound.Stop();
        }
    }

    void EndHunt()
    {
        Debug.Log("Ghost: Hunt ended, entering cooldown");
        SetState(GhostState.Cooldown);
        cooldownTimer = huntCooldown;

        // Make ghost invisible
        SetGhostVisibility(false);

        // Stop effects
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            ResetLights();
        }

        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);
        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);

        // Stop music
        if (huntingMusic != null) huntingMusic.Stop();
        if (heartbeatSound != null) heartbeatSound.Stop();

        // Return to spawn area
        SetRandomDestination();
    }

    #endregion

    #region Detection & Visibility

    bool CheckPlayerVisibility()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // Raycast check
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

    void SetGhostVisibility(bool visible)
    {
        // If alwaysVisible is on, never hide the ghost
        if (alwaysVisible) visible = true;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
        Debug.Log($"Ghost visibility: {visible}");
    }

    #endregion

    #region Light Flickering

    IEnumerator FlickerLights()
    {
        while (currentState == GhostState.HuntWarning || currentState == GhostState.Hunting)
        {
            foreach (Light light in allLights)
            {
                if (light != null)
                {
                    light.enabled = !light.enabled;
                }
            }
            yield return new WaitForSeconds(Random.Range(0.05f, flickerSpeed));
        }

        ResetLights();
    }

    void ResetLights()
    {
        foreach (Light light in allLights)
        {
            if (light != null)
            {
                light.enabled = true;
            }
        }
    }

    #endregion

    #region Movement

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    void SearchNearbyLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += lastKnownPlayerPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    #endregion

    #region Catch Player

    void CatchPlayer()
    {
        playerCaught = true;
        Debug.Log("Ghost: PLAYER CAUGHT - JUMPSCARE!");

        navAgent.isStopped = true;

        // Stop all effects
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        ResetLights();
        if (huntingMusic != null) huntingMusic.Stop();
        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);

        // Start jumpscare sequence
        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        // Make ghost visible
        SetGhostVisibility(true);

        // Get camera reference
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                playerCamera = xrOrigin.GetComponentInChildren<Camera>();
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError("No camera found for jumpscare!");
            yield return new WaitForSeconds(1f);
            LoadGameOver();
            yield break;
        }

        // Play jumpscare sound
        if (jumpscareSound != null)
        {
            jumpscareSound.Play();
        }
        else if (catchSound != null)
        {
            catchSound.Play();
        }

        // Show jumpscare image if available
        if (jumpscareImage != null)
        {
            jumpscareImage.SetActive(true);
        }

        // Move ghost face directly in front of player
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * jumpscareDistance;

        // Position ghost at player's eye level
        targetPosition.y = playerCamera.transform.position.y;
        transform.position = targetPosition;

        // Make ghost face the player correctly
        Vector3 lookDirection = playerCamera.transform.position - transform.position;
        lookDirection.y = 0; // Keep ghost upright
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Store original camera position for shake
        Vector3 originalCameraLocalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        // Jumpscare loop - shake screen and keep ghost in face
        while (elapsed < jumpscareDuration)
        {
            elapsed += Time.deltaTime;

            // Keep ghost in front of camera at eye level
            targetPosition = playerCamera.transform.position + playerCamera.transform.forward * jumpscareDistance;
            targetPosition.y = playerCamera.transform.position.y;
            transform.position = targetPosition;

            // Keep ghost facing player correctly
            lookDirection = playerCamera.transform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            // Screen shake effect
            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity;
            playerCamera.transform.localPosition = originalCameraLocalPos + new Vector3(shakeX, shakeY, 0);

            yield return null;
        }

        // Reset camera position
        playerCamera.transform.localPosition = originalCameraLocalPos;

        // Hide jumpscare image
        if (jumpscareImage != null)
        {
            jumpscareImage.SetActive(false);
        }

        // Load game over scene
        LoadGameOver();
    }

    void LoadGameOver()
    {
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

    #region State Management

    void SetState(GhostState newState)
    {
        currentState = newState;
        Debug.Log($"Ghost State: {newState}");
    }

    void UpdateAnimator()
    {
        if (ghostAnimator == null) return;

        bool isHunting = (currentState == GhostState.Hunting || currentState == GhostState.Searching);
        bool isMoving = navAgent.velocity.magnitude > 0.1f;

        ghostAnimator.SetBool("isHunting", isHunting);
        ghostAnimator.SetBool("isMoving", isMoving);
    }

    #endregion

    #region Public Methods

    public void ForceHunt()
    {
        if (currentState != GhostState.Hunting && currentState != GhostState.Cooldown)
        {
            StartHuntWarning();
        }
    }

    public void ResetGhost()
    {
        playerCaught = false;
        EndHunt();
        SetState(GhostState.Wandering);
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        // Wander radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // View distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // View cone
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Catch distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        // Line to player
        if (canSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
        }
    }

    #endregion
}