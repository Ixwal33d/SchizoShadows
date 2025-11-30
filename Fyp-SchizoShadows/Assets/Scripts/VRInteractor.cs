using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInteractor : MonoBehaviour
{
    public XRRayInteractor rayInteractor;

    private VRInteractable current;

    void Update()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            VRInteractable interactable = hit.collider.GetComponent<VRInteractable>();

            if (interactable != current)
            {
                if (current != null) current.Highlight(false);
                current = interactable;
                if (current != null) current.Highlight(true);
            }

            if (current != null && Input.GetKeyDown(KeyCode.E))
            {
                current.Interact();
            }
        }
    }
}
