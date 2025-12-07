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
        HuntWarning,
        Hunting,
        Searching,
        Cooldown
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
    public float huntCheckInterval = 30f;
    public float huntChanceMultiplier = 1f;
    private float huntCheckTimer = 0f;

    [Header("=== HUNT SETTINGS ===")]
    public float huntDuration = 30f;
    public float huntSpeed = 5f;
    public float huntWarningDuration = 3f;
    public float searchDuration = 10f;
    public float huntCooldown = 20f;
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
    public AudioSource heartbeatSound;
    public AudioSource huntingMusic;
    public AudioSource ghostBreathing;
    public AudioSource catchSound;

    [Header("=== DEBUG / TESTING ===")]
    public bool alwaysVisible = false;

    [Header("=== JUMPSCARE SETTINGS ===")]
    public float jumpscareDistance = 1f;
    public float jumpscareDuration = 2f;
    public float shakeIntensity = 0.3f;
    public float shakeSpeed = 30f;
    public AudioSource jumpscareSound;
    public GameObject jumpscareImage;

    [Header("=== SCREEN EFFECTS ===")]
    public GameObject huntWarningEffect;
    public GameObject huntActiveEffect;

    [Header("=== GAME OVER UI ===")]
    public GameObject gameOverPanel; // GAME OVER PANEL UPDATE

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

        if (sanityManager == null)
            sanityManager = FindObjectOfType<SanityManager>();

        if (player == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
                player = xrOrigin.transform;
            else
                player = Camera.main?.transform;
        }

        if (ghostAnimator == null)
            ghostAnimator = GetComponentInChildren<Animator>();

        if (autoFindLights)
            allLights.AddRange(FindObjectsOfType<Light>());

        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);
        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);

        // Hide Game Over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // GAME OVER PANEL UPDATE

        SetState(GhostState.Wandering);
        SetGhostVisibility(false);
    }

    void Update()
    {
        if (playerCaught || player == null) return;

        canSeePlayer = CheckPlayerVisibility();

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
            if (sanityPercent <= sanityThreshold)
            {
                float huntChance = (1f - sanityPercent) * huntChanceMultiplier;
                float roll = Random.Range(0f, 1f);

                if (roll <= huntChance)
                {
                    StartHuntWarning();
                }
            }
        }

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

        if (huntTimer <= 0)
        {
            EndHunt();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;
            navAgent.SetDestination(player.position);
        }
        else
        {
            navAgent.SetDestination(lastKnownPlayerPosition);

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

        if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            SearchNearbyLocation();
        }

        if (canSeePlayer)
        {
            SetState(GhostState.Hunting);
            huntTimer = huntDuration * 0.5f;
            return;
        }

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

    void StartHuntWarning()
    {
        SetState(GhostState.HuntWarning);
        warningTimer = huntWarningDuration;

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerLights());

        if (heartbeatSound != null) heartbeatSound.Play();
        if (huntWarningEffect != null) huntWarningEffect.SetActive(true);
    }

    void StartHunt()
    {
        SetState(GhostState.Hunting);
        huntTimer = huntDuration;

        SetGhostVisibility(true);
        lastKnownPlayerPosition = player.position;
        navAgent.SetDestination(player.position);
        navAgent.isStopped = false;

        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);
        if (huntActiveEffect != null) huntActiveEffect.SetActive(true);

        if (huntingMusic != null && !huntingMusic.isPlaying)
            huntingMusic.Play();

        if (heartbeatSound != null) heartbeatSound.Stop();
    }

    void EndHunt()
    {
        SetState(GhostState.Cooldown);
        cooldownTimer = huntCooldown;

        SetGhostVisibility(false);

        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            ResetLights();
        }

        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);
        if (huntWarningEffect != null) huntWarningEffect.SetActive(false);

        if (huntingMusic != null) huntingMusic.Stop();
        if (heartbeatSound != null) heartbeatSound.Stop();

        SetRandomDestination();
    }

    bool CheckPlayerVisibility()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 playerCenter = player.position + Vector3.up * 1f;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, (playerCenter - eyePos).normalized, out hit, viewDistance, obstacleLayer))
        {
            if (hit.transform != player && !hit.transform.IsChildOf(player)) return false;
        }

        return true;
    }

    void SetGhostVisibility(bool visible)
    {
        if (alwaysVisible) visible = true;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = visible;
    }

    IEnumerator FlickerLights()
    {
        while (currentState == GhostState.HuntWarning || currentState == GhostState.Hunting)
        {
            foreach (Light light in allLights)
            {
                if (light != null)
                    light.enabled = !light.enabled;
            }
            yield return new WaitForSeconds(Random.Range(0.05f, flickerSpeed));
        }

        ResetLights();
    }

    void ResetLights()
    {
        foreach (Light light in allLights)
        {
            if (light != null) light.enabled = true;
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    void SearchNearbyLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f + lastKnownPlayerPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    void CatchPlayer()
    {
        playerCaught = true;

        navAgent.isStopped = true;

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        ResetLights();

        if (huntingMusic != null) huntingMusic.Stop();
        if (huntActiveEffect != null) huntActiveEffect.SetActive(false);

        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        SetGhostVisibility(true);

        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
                playerCamera = xrOrigin.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            yield return new WaitForSeconds(1f);
            LoadGameOver();
            yield break;
        }

        if (jumpscareSound != null)
            jumpscareSound.Play();
        else if (catchSound != null)
            catchSound.Play();

        if (jumpscareImage != null)
            jumpscareImage.SetActive(true);

        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0;
        if (cameraForward.magnitude < 0.1f)
            cameraForward = Vector3.forward;

        cameraForward.Normalize();
        Vector3 targetPosition = playerCamera.transform.position + cameraForward * jumpscareDistance;
        targetPosition.y = playerCamera.transform.position.y;
        transform.position = targetPosition;

        Vector3 lookDirection = playerCamera.transform.position - transform.position;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        Quaternion correctRotation = transform.rotation;

        Vector3 originalCameraLocalPos = playerCamera.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < jumpscareDuration)
        {
            elapsed += Time.deltaTime;

            cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0;
            if (cameraForward.magnitude < 0.1f)
                cameraForward = Vector3.forward;

            cameraForward.Normalize();

            targetPosition = playerCamera.transform.position + cameraForward * jumpscareDistance;
            targetPosition.y = playerCamera.transform.position.y;
            transform.position = targetPosition;

            transform.rotation = correctRotation;

            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity;

            playerCamera.transform.localPosition =
                originalCameraLocalPos + new Vector3(shakeX, shakeY, 0);

            yield return null;
        }

        playerCamera.transform.localPosition = originalCameraLocalPos;

        if (jumpscareImage != null)
            jumpscareImage.SetActive(false);

        LoadGameOver();
    }

    // =============================
    //     GAME OVER PANEL UPDATE
    // =============================
    void LoadGameOver()
    {
        Debug.Log("GAME OVER - Showing UI Panel");

        // Disable player controller if exists
        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        // Show Game Over UI Panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Stop ghost completely
        if (navAgent != null)
            navAgent.isStopped = true;

        // Optional: Freeze game
        // Time.timeScale = 0f;
    }
    // =============================

    void SetState(GhostState newState)
    {
        currentState = newState;
    }

    void UpdateAnimator()
    {
        if (ghostAnimator == null) return;

        bool isMoving = navAgent.velocity.magnitude > 0.1f;
        ghostAnimator.SetBool("isHunting", isMoving);
        ghostAnimator.SetBool("isMoving", isMoving);
    }

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
}
