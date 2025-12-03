using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RectTransform of the Map Image UI")]
    public RectTransform mapUI;

    [Tooltip("The RectTransform of the 'You Are Here' Icon (should be a child of Map UI)")]
    public RectTransform playerIcon;

    [Tooltip("The Player's main Transform (VR Camera or Player Rig)")]
    public Transform playerTransform;

    [Header("Calibration")]
    [Tooltip("Place an Empty GameObject at the TOP-LEFT corner of your playable level")]
    public Transform worldTopLeftReference;

    [Tooltip("Place an Empty GameObject at the BOTTOM-RIGHT corner of your playable level")]
    public Transform worldBottomRightReference;

    void Update()
    {
        if (playerTransform == null || worldTopLeftReference == null || worldBottomRightReference == null)
            return;

        UpdatePlayerIconPosition();
    }

    void UpdatePlayerIconPosition()
    {
        // 1. Calculate the World Width and Height based on your reference points
        // We use X and Z because Y is height (up/down), which usually doesn't matter for 2D maps
        float worldWidth = worldBottomRightReference.position.x - worldTopLeftReference.position.x;
        float worldHeight = worldBottomRightReference.position.z - worldTopLeftReference.position.z;

        // 2. Calculate where the player is relative to the Top-Left reference (0 to 1 scale)
        float normalizedX = (playerTransform.position.x - worldTopLeftReference.position.x) / worldWidth;
        float normalizedY = (playerTransform.position.z - worldTopLeftReference.position.z) / worldHeight;

        // 3. Map dimensions
        float mapWidth = mapUI.rect.width;
        float mapHeight = mapUI.rect.height;

        // 4. Calculate the new position on the UI
        // Note: UI usually uses X and Y. World uses X and Z.
        Vector2 newAnchoredPos = new Vector2(
            normalizedX * mapWidth,
            normalizedY * mapHeight
        );

        // 5. Update the Icon
        // We assume the Icon's pivot is centered and parented to the Map Frame
        playerIcon.anchoredPosition = newAnchoredPos;
    }
}