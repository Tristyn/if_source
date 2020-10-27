using UnityEngine;

public sealed class SelectionHighlighter : MonoBehaviour
{
    public Color buildColor;
    public Color invalidColor;

    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int emissionColorId = Shader.PropertyToID("_EmissionColor");

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
        Color color = LandSystem.instance.CanBuild(bounds) ? buildColor : invalidColor;
        material.SetColor(baseColorId, color);
        material.SetColor(emissionColorId, color);

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
