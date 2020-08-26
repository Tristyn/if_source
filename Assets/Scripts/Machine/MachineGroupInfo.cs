using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMachineGroup", menuName = "Machine Group", order = 30)]
public sealed class MachineGroupInfo : ScriptableObject
{
    public string machineGroupName => name;
    public float groupOrder;
    public MachineInfo[] members;
    public MachineInfo[] assemblers;
    public MachineInfo[] purchasers;
    public MachineInfo[] sellers;

#if UNITY_EDITOR
    void OnValidate()
    {
        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (!masterList)
        {
            return;
        }

        if (!masterList.machineGroups.Contains(this))
        {
            masterList.machineGroups = masterList.machineGroups.Append(this);
        }

        masterList.machineGroups = masterList.machineGroups.OrderBy(machineInfo => machineInfo.groupOrder != 0 ? machineInfo.groupOrder : float.MaxValue).ToArray();

        BuildMembersArray();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(masterList);
    }

    public void BuildMembersArray()
    {
        members = assemblers.Concat(purchasers).Concat(sellers).ToArray();
    }
#endif
}
