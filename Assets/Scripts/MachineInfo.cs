#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct AssembleSlot
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
    public Vector3Int size = new Vector3Int(2, 1, 2);
    public GameObject prefab;
    public Color color = Color.white;
    public Sprite sprite;
    public Color spriteColor = Color.white;

    public Inventory inventory = new Inventory { slots = new InventorySlot[0] };

    public AssembleSlot purchaseItem;
    public AssembleSlot sellItem;

    public bool assembler;

    public AssembleSlot[] assembleInputs;
    public AssembleSlot assembleOutput;
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


        // inventory order is purchase item, sell item, assemble inputs, assemble outputs, anything else
        List<InventorySlot> inventorySlots = new List<InventorySlot>();
        if (purchaseItem.itemInfo != null)
        {
            inventorySlots.Add(InventorySlot.Defaults(purchaseItem));
        }
        if (sellItem.itemInfo != null)
        {
            inventorySlots.Add(InventorySlot.Defaults(sellItem));
        }
        if (assembler)
        {
            inventorySlots.AddRange(assembleInputs.Select(item => InventorySlot.Defaults(item.itemInfo)));
            inventorySlots.Add(InventorySlot.Defaults(assembleOutput.itemInfo));
        }
        inventorySlots.AddRange(inventory.slots.Except(inventorySlots.ToArray()));
        inventory = new Inventory
        {
            slots = inventorySlots.ToArray()
        };

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(masterList);
    }
#endif
}