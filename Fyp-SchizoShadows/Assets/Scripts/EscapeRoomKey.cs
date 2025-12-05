using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

/// <summary>
/// Escape Room Key for SimpleDoorVR
/// Locks in hand until used on door
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class EscapeRoomKey : MonoBehaviour
{
    [Header("Key Identity")]
    [SerializeField] private string keyID = "Key_1";
    [SerializeField] private string keyName = "Old Key";
    [SerializeField] private bool isCorrectKey = false;
    
    [Header("Visual")]
    [SerializeField] private Color keyColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip wrongKeySound;
    
    [Header("Settings")]
    [SerializeField] private bool lockInHand = true;
    [SerializeField] private float doorDetectDistance = 3f;
    
    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private Rigidbody keyRigidbody;
    private bool isLockedInHand = false;
    private bool hasBeenPickedUp = false;

    void Awake()
    {
        Debug.Log($"🔑 Key '{keyName}' initializing...");
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError($"❌ {keyName}: Missing XRGrabInteractable!");
        }
        
        keyRigidbody = GetComponent<Rigidbody>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = keyColor;
        }
        
        Debug.Log($"✅ {keyName} ready - IsCorrectKey: {isCorrectKey}");
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log($"✋ {keyName} GRABBED!");
        
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;

            if (keyRigidbody != null)
            {
                keyRigidbody.isKinematic = false;
                keyRigidbody.constraints = RigidbodyConstraints.None;
            }

            if (pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            if (lockInHand)
            {
                isLockedInHand = true;
                Debug.Log($"🔒 {keyName} LOCKED in hand!");
            }
        }
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        if (isLockedInHand)
        {
            Debug.Log($"⚠️ {keyName} is locked - attempting re-grab...");
            Invoke("TryReGrab", 0.1f);
        }
    }
    
    void TryReGrab()
    {
        if (!grabInteractable.isSelected && isLockedInHand)
        {
            XRBaseInteractor[] interactors = FindObjectsOfType<XRBaseInteractor>();
            
            foreach (var interactor in interactors)
            {
                float distance = Vector3.Distance(interactor.transform.position, transform.position);
                if (distance < 0.5f)
                {
                    interactor.interactionManager.SelectEnter(interactor, grabInteractable);
                    Debug.Log($"✅ {keyName} re-grabbed!");
                    return;
                }
            }
        }
    }

    void Update()
    {
        if (isLockedInHand && hasBeenPickedUp && grabInteractable.isSelected)
        {
            CheckNearbyLockedDoors();
        }
    }

    void CheckNearbyLockedDoors()
    {
        SimpleDoor[] allDoors = FindObjectsOfType<SimpleDoor>();
        
        if (allDoors.Length == 0)
        {
            Debug.LogWarning("⚠️ No SimpleDoorVR doors found in scene!");
            return;
        }
        
        foreach (SimpleDoor door in allDoors)
        {
            if (!door.IsLocked()) continue;
            
            float distance = Vector3.Distance(transform.position, door.transform.position);
            
            if (distance < doorDetectDistance)
            {
                Debug.Log($"💡 {keyName} near locked door! Distance: {distance:F2}m");
                
                if (isCorrectKey && keyID == door.GetRequiredKeyID())
                {
                    Debug.Log($"✅ CORRECT KEY! {keyName} unlocks door!");
                    door.UnlockWithKey(keyID);
                    isLockedInHand = false;
                    ForceDropKey();
                }
                else
                {
                    Debug.Log($"❌ WRONG KEY! {keyName} doesn't work!");
                    if (wrongKeySound != null) audioSource.PlayOneShot(wrongKeySound);
                    isLockedInHand = false;
                    ForceDropKey();
                }
                
                return;
            }
        }
    }

    void ForceDropKey()
    {
        Debug.Log($"🔓 Dropping {keyName}...");
        
        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting as XRBaseInteractor;
            if (interactor != null)
            {
                interactor.interactionManager.SelectExit(interactor, grabInteractable);
                Debug.Log($"✅ {keyName} dropped!");
                return;
            }
        }
        
        StartCoroutine(TemporaryDisableGrab());
    }
    
    IEnumerator TemporaryDisableGrab()
    {
        var originalMask = grabInteractable.interactionLayers;
        grabInteractable.interactionLayers = 0;
        
        yield return new WaitForSeconds(0.1f);
        
        grabInteractable.interactionLayers = originalMask;
        Debug.Log($"✅ {keyName} can be grabbed again!");
    }

    public bool IsCorrectKey()
    {
        return isCorrectKey;
    }

    public string GetKeyID()
    {
        return keyID;
    }
    
    public bool IsLockedInHand()
    {
        return isLockedInHand;
    }
}