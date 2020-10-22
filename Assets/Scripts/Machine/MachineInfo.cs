#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public struct AssembleSlot
{
    public ItemInfo itemInfo;
    public int count;
}

[CreateAssetMenu(fileName = "NewMachine", menuName = "Machine", order = 30)]
public sealed class MachineInfo : ScriptableObject
{
    public string machineName => name;
    public MachineGroupInfo machineGroup;
    public long cost;
    public float placeInterval = 1;
    public Vector3Int size = new Vector3Int(2, 1, 2);
#pragma warning disable 0649
    [SerializeField]
    MachineVisual _prefab;
#pragma warning restore 0649
    [NonSerialized]
    public MachineVisual prefab;
    public Sprite sprite;

    public Color spriteColor
    {
        get
        {
            if (sellItem.itemInfo)
            {
                return sellItem.itemInfo.color;
            }
            if (assembler && assembleOutput.itemInfo)
            {
                return assembleOutput.itemInfo.color;
            }
            if (purchaseItem.itemInfo)
            {
                return purchaseItem.itemInfo.color;
            }
            return Color.white;
        }
    }

    public AssembleSlot purchaseItem;
    public AssembleSlot sellItem;

    public bool assembler;

    public AssembleSlot[] assembleInputs;
    public AssembleSlot assembleOutput;

    public void Initialize()
    {
        if (_prefab)
        {
            prefab = _prefab;
        }
        else
        {
            prefab = ScriptableObjects.instance.machineVisualDefault;
        }
    }

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

        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (masterList)
        {
            if (!masterList.machines.Any(machine => machine.machineName == machineName))
            {
                masterList.machines = masterList.machines.Append(this);
                EditorUtility.SetDirty(masterList);
            }
        }

        if (machineGroup)
        {
            if (assembler && !machineGroup.assemblers.Contains(this))
            {
                machineGroup.assemblers = machineGroup.assemblers.Append(this).ToArray();
            }
            if (purchaseItem.itemInfo != null && !machineGroup.purchasers.Contains(this))
            {
                machineGroup.purchasers = machineGroup.purchasers.Append(this).ToArray();
            }
            if (sellItem.itemInfo != null && !machineGroup.sellers.Contains(this))
            {
                machineGroup.sellers = machineGroup.sellers.Append(this).ToArray();
            }

            machineGroup.BuildMembersArray();
            EditorUtility.SetDirty(machineGroup);
        }
        else
        {
            for (int i = 0, len = masterList.machineGroups.Length; i < len; i++)
            {
                MachineGroupInfo machineGroup = masterList.machineGroups[i];
                if (machineGroup.members.Contains(this))
                {
                    if (machineGroup.assemblers.Contains(this))
                    {
                        machineGroup.assemblers = machineGroup.assemblers.Except(this).ToArray();
                    }
                    if (machineGroup.purchasers.Contains(this))
                    {
                        machineGroup.purchasers = machineGroup.purchasers.Except(this).ToArray();
                    }
                    if (machineGroup.sellers.Contains(this))
                    {
                        machineGroup.sellers = machineGroup.sellers.Except(this).ToArray();
                    }
                    machineGroup.BuildMembersArray();
                    EditorUtility.SetDirty(machineGroup);
                }
            }
        }

        EditorUtility.SetDirty(this);
    }
#endif
}