using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    public Material[] materials;

    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>() ?? GetComponent<Renderer>();
    }

    public void SetMaterial(int index)
    {
        if (!_renderer)
        {
            Debug.LogWarning("No renderer found");
            return;
        }

        if (index >= materials.Length)
        {
            Debug.LogWarning("Material index out of bounds");
            return;
        }

        _renderer.material = materials[index];
    }
}
