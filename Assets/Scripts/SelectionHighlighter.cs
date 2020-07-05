using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SelectionHighlighter : MonoBehaviour
{
    GameObject[] selectionHighlighters;
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
        transform.position = center;
        transform.localScale = size;
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
