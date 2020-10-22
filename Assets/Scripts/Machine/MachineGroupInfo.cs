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
        assemblers = assemblers.Where(machine => machine).ToArray();
        purchasers = purchasers.Where(machine => machine).ToArray();
        sellers = sellers.Where(machine => machine).ToArray();
        BuildMembersArray();

        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (masterList)
        {
            if (!masterList.machineGroups.Contains(this))
            {
                masterList.machineGroups = masterList.machineGroups.Append(this);
            }

            masterList.machineGroups = masterList.machineGroups.OrderBy(machineInfo => machineInfo.groupOrder != 0 ? machineInfo.groupOrder : float.MaxValue).ToArray();

            EditorUtility.SetDirty(masterList);
        }
        EditorUtility.SetDirty(this);
    }

    public void BuildMembersArray()
    {
        members = purchasers.Concat(assemblers).Concat(sellers).ToArray();
    }
#endif
}
