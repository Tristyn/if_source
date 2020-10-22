﻿#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item", order = 30)]
public sealed class ItemInfo : ScriptableObject
{
    public string itemName => name;
    public long value;
    public Color color = Color.white;
    public Item prefab;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!prefab)
        {
            string[] guids = AssetDatabase.FindAssets("Item t:GameObject", new[] { "Assets/Prefabs" }).ToArray();
            guids = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            guids = guids.Where(path2 => path2.EndsWith("/Item.prefab")).ToArray();
            string path = guids.FirstOrDefault();
            if (path != null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<Item>(path);
            }
        }

        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (!masterList)
        {
            return;
        }

        if (!masterList.items.Any(item => item && item.itemName == itemName))
        {
            masterList.items = masterList.items.Append(this);
        }

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(masterList);
    }
#endif
}