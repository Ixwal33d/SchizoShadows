using UnityEngine;

/// <summary>
/// Debug Helper - Add to any object to check key/door setup
/// This will tell you exactly what's wrong
/// </summary>
public class KeyDoorDebugChecker : MonoBehaviour
{
    [Header("Press Space to Run Check")]
    [SerializeField] private KeyCode checkKey = KeyCode.Space;
    
    void Update()
    {
        if (Input.GetKeyDown(checkKey))
        {
            RunFullDiagnostic();
        }
    }

    void Start()
    {
        Debug.Log("🔍 KeyDoorDebugChecker active! Press SPACE to run diagnostic.");
        Invoke(nameof(RunFullDiagnostic), 2f); // Auto-run after 2 seconds
    }

    void RunFullDiagnostic()
    {
        Debug.Log("=====================================");
        Debug.Log("🔍 KEY & DOOR DIAGNOSTIC CHECK");
        Debug.Log("=====================================");
        
        CheckAllDoors();
        CheckAllKeys();
        CheckMatching();
        
        Debug.Log("=====================================");
        Debug.Log("✅ Diagnostic Complete!");
        Debug.Log("=====================================");
    }

    void CheckAllDoors()
    {
        Debug.Log("\n🚪 CHECKING ALL DOORS:");
        Debug.Log("-------------------------------------");
        
        UniversalDoor[] doors = FindObjectsOfType<UniversalDoor>();
        
        if (doors.Length == 0)
        {
            Debug.LogError("❌ NO DOORS FOUND! Add UniversalDoor script to doors!");
            return;
        }
        
        Debug.Log($"Found {doors.Length} door(s):");
        
        int lockedCount = 0;
        int unlockedCount = 0;
        
        foreach (UniversalDoor door in doors)
        {
            if (door.IsLocked())
            {
                lockedCount++;
                Debug.Log($"  🔒 '{door.gameObject.name}' - LOCKED");
                Debug.Log($"      Required Key ID: '{door.GetRequiredKeyID()}'");
                
                // Check components
                if (!door.GetComponent<Rigidbody>())
                {
                    Debug.LogWarning($"      ⚠️ Missing Rigidbody!");
                }
                if (!door.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>())
                {
                    Debug.LogWarning($"      ⚠️ Missing XR Grab Interactable!");
                }
            }
            else
            {
                unlockedCount++;
                Debug.Log($"  ✅ '{door.gameObject.name}' - UNLOCKED");
            }
        }
        
        Debug.Log($"\nSummary: {lockedCount} locked, {unlockedCount} unlocked");
        
        if (lockedCount == 0)
        {
            Debug.LogWarning("⚠️ WARNING: No locked doors! Set one door's 'Is Locked' to TRUE!");
        }
        if (lockedCount > 1)
        {
            Debug.LogWarning($"⚠️ WARNING: {lockedCount} locked doors! Usually only 1 should be locked.");
        }
    }

    void CheckAllKeys()
    {
        Debug.Log("\n🔑 CHECKING ALL KEYS:");
        Debug.Log("-------------------------------------");
        
        EscapeRoomKey[] keys = FindObjectsOfType<EscapeRoomKey>();
        
        if (keys.Length == 0)
        {
            Debug.LogError("❌ NO KEYS FOUND! Add EscapeRoomKey script to keys!");
            return;
        }
        
        Debug.Log($"Found {keys.Length} key(s):");
        
        int correctCount = 0;
        int wrongCount = 0;
        
        foreach (EscapeRoomKey key in keys)
        {
            if (key.IsCorrectKey())
            {
                correctCount++;
                Debug.Log($"  ✅ '{key.gameObject.name}' - CORRECT KEY");
                Debug.Log($"      Key ID: '{key.GetKeyID()}'");
            }
            else
            {
                wrongCount++;
                Debug.Log($"  ❌ '{key.gameObject.name}' - Wrong key");
                Debug.Log($"      Key ID: '{key.GetKeyID()}'");
            }
            
            // Check components
            if (!key.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>())
            {
                Debug.LogWarning($"      ⚠️ Missing XR Grab Interactable!");
            }
        }
        
        Debug.Log($"\nSummary: {correctCount} correct, {wrongCount} wrong");
        
        if (correctCount == 0)
        {
            Debug.LogError("❌ ERROR: No correct key! Set one key's 'Is Correct Key' to TRUE!");
        }
        if (correctCount > 1)
        {
            Debug.LogWarning($"⚠️ WARNING: {correctCount} correct keys! Usually only 1 should be correct.");
        }
    }

    void CheckMatching()
    {
        Debug.Log("\n🔍 CHECKING KEY-DOOR MATCHING:");
        Debug.Log("-------------------------------------");
        
        UniversalDoor[] doors = FindObjectsOfType<UniversalDoor>();
        EscapeRoomKey[] keys = FindObjectsOfType<EscapeRoomKey>();
        
        // Find locked doors
        foreach (UniversalDoor door in doors)
        {
            if (door.IsLocked())
            {
                string requiredKeyID = door.GetRequiredKeyID();
                Debug.Log($"\n🔒 Door '{door.gameObject.name}' needs: '{requiredKeyID}'");
                
                // Find matching key
                bool foundMatch = false;
                foreach (EscapeRoomKey key in keys)
                {
                    if (key.GetKeyID() == requiredKeyID && key.IsCorrectKey())
                    {
                        foundMatch = true;
                        Debug.Log($"   ✅ MATCH FOUND: '{key.gameObject.name}' (Key ID: '{key.GetKeyID()}')");
                    }
                }
                
                if (!foundMatch)
                {
                    Debug.LogError($"   ❌ NO MATCHING KEY FOUND!");
                    Debug.LogError($"   FIX: Set a key's 'Key ID' to '{requiredKeyID}' and check 'Is Correct Key'!");
                }
            }
        }
    }
}