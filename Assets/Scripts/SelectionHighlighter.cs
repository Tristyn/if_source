using UnityEngine;

public sealed class SelectionHighlighter : MonoBehaviour
{
    Material material;

    private void Awake()
    {
        material = GetComponentInChildren<Renderer>().material;
    }

    public void Initialize(Bounds3Int bounds)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        center.y = bounds.min.y;
        material.mainTextureScale = new Vector2(size.x, size.z);
        transform.localPosition = center;
        transform.localScale = size;
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }

    private void OnDestroy()
    {
        Destroy(material);
    }
}
