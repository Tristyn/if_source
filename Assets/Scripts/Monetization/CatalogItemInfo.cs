using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewCatalogItem", menuName = "Catalog Item", order = 30)]
public sealed class CatalogItemInfo : ScriptableObject
{
    public string itemId;
    public uint quantity = 1;
    public string catalogVersion = "IAP1.0";
    public bool isProfileBound = false;

#if UNITY_EDITOR
    void OnValidate()
    {
        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (!masterList)
        {
            return;
        }

        if (!masterList.catalogItemInfos.Contains(this))
        {
            masterList.catalogItemInfos = masterList.catalogItemInfos.Append(this);
            EditorUtility.SetDirty(masterList);
        }

        EditorUtility.SetDirty(this);
    }
#endif
}
