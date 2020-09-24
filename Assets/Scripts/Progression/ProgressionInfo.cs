using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "ProgressionInfo", menuName = "Progression Info", order = 30)]
public class ProgressionInfo : ScriptableObject
{
    public string progressionName;
    [ReadOnly]
    public int progressionId;

    public MachineInfo[] machineUnlockConditions;
    [NonSerialized]
    public BitArray32 machineUnlockConditionsMet;
    public long moneyCondition;
    public long levelCondition;

    public long moneyReward;
    public long xpReward;
    public MachineInfo[] machineUnlockRewards;

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
        if (progressionId == 0)
        {
            ScriptableObjectMasterList masterList = ScriptableObjectMasterList.LoadAsset();
            masterList.nextProgressionId++;
            progressionId = masterList.nextProgressionId++;
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(masterList);
        }
    }
#endif
}