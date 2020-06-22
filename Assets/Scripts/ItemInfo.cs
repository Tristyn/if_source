#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item", order = 30)]
public class ItemInfo : ScriptableObject
{
    public string itemName => name;
    public float cost;
    public float placeInterval;
    public Color color = Color.white;
    public GameObject prefab;

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
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
    }
#endif
}