using TMPro;
using UnityEngine;

public sealed class MachineVisual : MonoBehaviour
{
    public Renderer[] machineRenderers;
    public Renderer categoryRenderer;
    public Renderer textRenderer;
    public TextMeshPro text;
    public Material purchaserCategory;
    public Material assemblerCategory;
    public Material sellerCategory;

    public MachineInfo machineInfo;

    public float smallScale;
    public bool resizeMachineRenderers;
    public bool colorMachineMaterials;
    public bool tileMachineMaterials;

    public static MachineVisual Create(MachineInfo machineInfo, Transform parent)
    {
        MachineVisual instance = Instantiate(machineInfo.prefab, parent);
        instance.machineInfo = machineInfo;
        instance.Initialize();
        return instance;
    }

    public void Initialize()
    {
        if (machineInfo)
        {
            Vector3 position_local = machineInfo.size.Scale(0.5f);
            Vector3 scale_local = machineInfo.size;
            Vector2 textureScale = new Vector2(scale_local.x, scale_local.z);
            bool smallText = scale_local.x < 2 || scale_local.z < 2;

            for (int i = 0, len = machineRenderers.Length; i < len; ++i)
            {
                Renderer machineRenderer = machineRenderers[i];
                if (colorMachineMaterials)
                {
                    machineRenderer.material.color = machineInfo.spriteColor;
                }
                if (tileMachineMaterials)
                {
                    machineRenderer.material.mainTextureScale = textureScale;
                }
                if (resizeMachineRenderers)
                {
                    Transform machineTransform = machineRenderer.transform;
                    machineTransform.localPosition = position_local;
                    machineTransform.localScale = scale_local;
                }
            }
            if (resizeMachineRenderers)
            {
                text.margin = new Vector4(0.13f, 0.13f, 0.13f, 0.13f);
                RectTransform textTransform = (RectTransform)textRenderer.transform;
                textTransform.anchoredPosition3D = new Vector3(0, scale_local.y + 0.05f, scale_local.z);
                textTransform.sizeDelta = new Vector2(textureScale.y, textureScale.x);// Hack 
                if (smallText)
                {
                    text.fontSize = text.fontSize * smallScale;
                    categoryRenderer.transform.localScale *= smallScale;
                    float min = Mathf.Min(position_local.x, position_local.z);
                    categoryRenderer.transform.localPosition = new Vector3(min, scale_local.y + 0.05f, min);
                }
            }

            Material categoryMaterial = null;
            if (machineInfo.purchaseItem.itemInfo)
            {
                categoryMaterial = purchaserCategory;
            }
            else if (machineInfo.sellItem.itemInfo)
            {
                categoryMaterial = sellerCategory;
            }
            else if (machineInfo.assembler)
            {
                categoryMaterial = assemblerCategory;
            }
            if (categoryRenderer)
            {
                categoryRenderer.material = categoryMaterial;
            }
            if (text)
            {
                text.text = machineInfo.machineName;
            }
        }
    }
}
