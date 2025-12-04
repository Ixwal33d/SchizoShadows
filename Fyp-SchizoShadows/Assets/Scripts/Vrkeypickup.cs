using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRKeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "MainKey";
    [SerializeField] private AudioClip pickupSound;

    private XRGrabInteractable grabInteractable;
    private SubtleGlowKey subtleGlowKey;
    private AudioSource audioSource;
    private bool hasBeenPickedUp = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        subtleGlowKey = GetComponent<SubtleGlowKey>();

        // Setup audio source for pickup sound
        if (pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = pickupSound;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnPickup);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnPickup);
    }

    void OnPickup(SelectEnterEventArgs args)
    {
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;

            // Stop the glow effect
            if (subtleGlowKey != null)
            {
                subtleGlowKey.DisableGlow();
            }

            // Play pickup sound
            if (audioSource != null && pickupSound != null)
            {
                audioSource.Play();
            }

            // Add key to inventory or trigger event
            Debug.Log("Key picked up: " + keyID);

            // You can add your own inventory system call here
            // Example: InventoryManager.Instance.AddKey(keyID);
        }
    }
}
