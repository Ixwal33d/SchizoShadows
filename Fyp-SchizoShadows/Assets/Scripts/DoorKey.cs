using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorKey : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SimpleDoor door = other.GetComponent<SimpleDoor>();
        if (door != null)
        {
            door.UnlockWithKey();
        }
    }
}