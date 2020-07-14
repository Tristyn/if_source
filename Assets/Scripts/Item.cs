using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    
    public void Initialize()
    {
        if (itemInfo.color != Color.white)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0, len = renderers.Length; i < len; ++i)
            {
                renderers[i].material.color = itemInfo.color;
            }
        }
    }

    public void Recycle()
    {
        ItemPooler.instance.Recycle(this);
    }

    public void ConsumedByMachine()
    {
        Recycle();
    }

    public void EvictedFromConveyor()
    {
        Recycle();
    }
}
