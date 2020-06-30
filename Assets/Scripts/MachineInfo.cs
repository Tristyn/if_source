#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public struct AssembleInfo
{
    public ItemInfo itemInfo;
    public int count;
}

[CreateAssetMenu(fileName = "NewMachine", menuName = "Machine", order = 30)]
public class MachineInfo : ScriptableObject
{
    public string machineName => name;
    public string machineGroup = string.Empty;
    public float groupOrder;
    public float cost;
    public float placeInterval;
    public Vector3Int size = new Vector3Int(2,1,2);
    public GameObject prefab;
    public Color color = Color.white;
    public Sprite sprite;
    public Color spriteColor = Color.white;

    public ItemInfo purchaseItem;
    public ItemInfo sellItem;

    public bool assembler;

    public AssembleInfo[] assembleInputs;
    public AssembleInfo[] assembleOutputs;
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!sprite)
        {
            string[] guids = AssetDatabase.FindAssets("Machine t:Sprite", new[] { "Assets/MaterialTextures" }).ToArray();
            guids = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            guids = guids.Where(path2 => path2.EndsWith("/Machine.psd")).ToArray();
            string path = guids.FirstOrDefault();
            if (path != null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }

        ScriptableObjectMasterList masterList = AssetDatabase.LoadAssetAtPath<ScriptableObjectMasterList>(
            AssetDatabase.FindAssets("ObjectMasterList t:ScriptableObjectMasterList", new[] { "Assets/Objects" })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .SingleOrDefault());
        if (!masterList.allMachines.Contains(this))
        {
            masterList.allMachines = masterList.allMachines.Append(this).ToArray();
        }

        masterList.GroupMachines();

        if (machineGroup != string.Empty)
        {
            if (groupOrder != 0)
            {
                masterList.SetMachineGroupOrder(machineGroup, groupOrder);
            }
            else
            {
                for (int i = 0, len = masterList.machineGroups.Length; i < len; i++)
                {
                    MachineGroup group = masterList.machineGroups[i];
                    if (group.groupName == machineGroup)
                    {
                        groupOrder = group.groupOrder;
                    }
                }
            }
        }
    }
#endif
}