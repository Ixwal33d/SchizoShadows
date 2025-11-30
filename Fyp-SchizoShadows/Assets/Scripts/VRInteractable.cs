using UnityEngine;

public class VRInteractable : MonoBehaviour
{
    public Material highlightMaterial;
    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    public void Highlight(bool on)
    {
        if (on)
            meshRenderer.material = highlightMaterial;
        else
            meshRenderer.material = originalMaterial;
    }

    public void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.name);
       
    }
}
