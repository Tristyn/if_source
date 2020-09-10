using UnityEngine;

public class Floor : MonoBehaviour
{
    public Color color;
    public void Initialize(Bounds3Int parcelBounds)
    {
        Vector3 position_local = parcelBounds.bottomCenter;
        Vector3Int scale = parcelBounds.size;
        scale.y = 1;

        Transform transform = this.transform;
        transform.localPosition = position_local;
        transform.localScale = scale;

        if(color != Color.white)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for(int i = 0,len = renderers.Length; i < len; ++i)
            {
                renderers[i].material.color *= color;
            }
        }
    }
}
