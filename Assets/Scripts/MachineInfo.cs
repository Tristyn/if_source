#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMachine", menuName = "Machine", order = 30)]
public class MachineInfo : ScriptableObject
{
    public string machineName => name;
    public float cost;
    public float placeInterval;
    public Color32? color;
    public GameObject prefab;

    public ItemInfo purchaseItem;
    public ItemInfo sellItem;

    public bool assembler;

    public List<AssembleInfo> assembleInputs;
    public List<AssembleInfo> assembleOutputs;
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!prefab)
        {
            string[] guids = AssetDatabase.FindAssets("Machine t:GameObject", new[] { "Assets/Prefabs" }).ToArray();
            guids = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            guids = guids.Where(path2 => path2.EndsWith("/Machine.prefab")).ToArray();
            string path = guids.FirstOrDefault();
            if (path != null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
    }
#endif
}

public class AssembleInfo
{
    public ItemInfo itemInfo;
    public int count;
}