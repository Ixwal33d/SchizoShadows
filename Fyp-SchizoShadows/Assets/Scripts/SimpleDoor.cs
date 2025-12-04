using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    public float openAngle = -90f;
    public float openSpeed = 2f;
    public bool requiresKey = false;
    
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }
    
    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }
    
    public void ToggleDoor()
    {
        if (!requiresKey)
        {
            isOpen = !isOpen;
        }
    }
    
    public void UnlockWithKey()
    {
        requiresKey = false;
        isOpen = true;
    }
}