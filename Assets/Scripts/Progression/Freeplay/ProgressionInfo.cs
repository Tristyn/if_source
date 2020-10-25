using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "ProgressionInfo", menuName = "Progression Info", order = 30)]
public sealed class ProgressionInfo : ScriptableObject
{
    public string progressionName;
    [ReadOnly]
    public int progressionId;

    public MachineInfo[] machineUnlockConditions;
    public long moneyCondition;
    public long levelCondition;

    public long moneyReward;
    public long xpReward;
    public MachineInfo[] machineRewards;
    public MachineGroupInfo[] machineGroupRewards;

#if UNITY_EDITOR
    void OnValidate()
    {
        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (!masterList)
        {
            return;
        }

        if (!masterList.progressionInfos.Contains(this))
        {
            masterList.progressionInfos = masterList.progressionInfos.Append(this);
            masterList.progressionInfos = masterList.progressionInfos.Where(progressionInfo => progressionInfo).ToArray();
            EditorUtility.SetDirty(masterList);
        }

        ValidateProgressionId();
    }

    public void ValidateProgressionId()
    {
        ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
        if (masterList && masterList.progressionInfos.Count(progressionInfo => progressionInfo.progressionId == progressionId) > 1)
        {
            progressionId = masterList.nextProgressionId++;
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(masterList);
        }
    }
#endif
}