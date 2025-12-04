using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetZ;
    [SerializeField] private float LerpSpeed;

    // Use LateUpdate for camera or minimap following to ensure the target has
    // finished moving in the Update phase.
    private void LateUpdate()
    {
        // Smoothly move the current object (camera/minimap) from its current position 
        // to a new target position.
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(
                target.position.x + offsetX,
                transform.position.y,
                target.position.z + offsetZ
            ),
            LerpSpeed
        );
    }
}