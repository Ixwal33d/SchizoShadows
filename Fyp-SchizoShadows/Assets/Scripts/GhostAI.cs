using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // drag XR Origin / Player transform here
    public SanityManager sanityManager;      // drag the SanityManager instance here
    public Animator animator;                // ghost Animator (Mixamo controller)

    [Header("Behavior")]
    public float activateSanityThreshold = 70f; // when currentSanity <= this, ghost activates
    public float chaseDistance = 999f;         // optional distance limit (set large to ignore)
    public bool onlyAppearWhenActive = true;   // if true, ghost GameObject will be disabled until active

    NavMeshAgent agent;
    bool isActive = false;

    // cache previous state so we only toggle when threshold crosses
    float prevSanity = -1f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (sanityManager == null) sanityManager = FindObjectOfType<SanityManager>();
        if (player == null && Camera.main != null)
        {
            // try to find an XR rig root (you should assign manually for reliability)
            player = Camera.main.transform;
        }

        if (onlyAppearWhenActive) gameObject.SetActive(false); // start hidden
    }

    void Update()
    {
        if (sanityManager == null || player == null) return;

        // current sanity value (not percent)
        float current = sanityManager.currentSanity;

        // first run, cache
        if (prevSanity < 0f) prevSanity = current;

        // check threshold crossing (activate when <= threshold)
        if (!isActive && current <= activateSanityThreshold)
        {
            ActivateGhost();
        }
        else if (isActive && current > activateSanityThreshold)
        {
            DeactivateGhost();
        }

        prevSanity = current;

        if (!isActive) return;

        // distance guard (optional)
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > chaseDistance)
        {
            agent.ResetPath();
            if (animator != null) animator.SetBool("isRunning", false);
            return;
        }

        // chase the player
        agent.SetDestination(player.position);
        if (animator != null) animator.SetBool("isRunning", true);
    }

    void ActivateGhost()
    {
        isActive = true;
        if (onlyAppearWhenActive && !gameObject.activeSelf) gameObject.SetActive(true);
        agent.enabled = true;
        // optional: set agent speed higher for "hunt" feel
        agent.speed = Mathf.Max(agent.speed, 3.5f);
        // play a spawn animation or enable visual
        if (animator != null) animator.SetTrigger("Appear");
        Debug.Log($"Ghost activated (sanity {sanityManager.currentSanity})");
    }

    void DeactivateGhost()
    {
        isActive = false;
        agent.ResetPath();
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Idle");
        }
        // optionally hide ghost when sanity recovers
        if (onlyAppearWhenActive) gameObject.SetActive(false);
        Debug.Log($"Ghost deactivated (sanity {sanityManager.currentSanity})");
    }
}
