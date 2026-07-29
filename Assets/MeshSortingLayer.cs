using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MeshSortingLayer : MonoBehaviour
{
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int orderInLayer = 0;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        ApplySorting();
    }

    private void OnValidate()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
            ApplySorting();
    }

    private void ApplySorting()
    {
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = orderInLayer;
    }
}