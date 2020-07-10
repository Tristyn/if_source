using UnityEngine;

public class MachineCreationVisualizer : MonoBehaviour
{
    public Material hologramMaterial;
    public SelectionHighlighter selectionHighlighter;
    
    MachineInfo machineInfo;
    MachineVisual instance;

    Bounds3Int bounds;

    public void Visualize(MachineInfo machineInfo, Vector3 center)
    {
        if (machineInfo != this.machineInfo)
        {
            this.machineInfo = machineInfo;
            if (instance)
            {
                Destroy(instance.gameObject);
            }
            InitializeHologram();
        }

        Bounds3Int bounds = center.PositionBottomToBounds(machineInfo.size);
        this.bounds = bounds;
        instance.transform.localPosition = bounds.min;
        selectionHighlighter.Initialize(bounds);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(bounds.center, new Vector3(0.4f, 0.4f, 0.4f));
        Gizmos.DrawCube(bounds.min, new Vector3(0.4f, 0.4f, 0.4f));
    }

    void InitializeHologram()
    {
        instance = Instantiate(machineInfo.prefab, transform);
        Renderer[] machineRenderers = instance.machineRenderers;
        for (int i = 0, len = machineRenderers.Length; i < len; i++)
        {
            Renderer machineRenderer = machineRenderers[i];
            machineRenderer.sharedMaterial = hologramMaterial;
            machineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Renderer[] infoRenderers = instance.infoRenderers;
        for (int i = 0, len = infoRenderers.Length; i < len; i++)
        {
            infoRenderers[i].enabled = false;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void Recycle()
    {
        Destroy(instance);
        ObjectPooler.instance.Recycle(this);
    }
}
