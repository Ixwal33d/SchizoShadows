using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT: " + other.name);
    }
}
