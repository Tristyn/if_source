using System.Collections.Generic;

public static class ProgressionApplyer
{
    public static void ApplyRewards(List<ProgressionInfo> progressionInfos)
    {
        for (int i = 0, len = progressionInfos.Count; i < len; ++i)
        {
            ApplyRewards(progressionInfos[i]);
        }
    }

    public static void ApplyRewards(ProgressionInfo progressionInfo)
    {
        if (CanApplyRewards(progressionInfo))
        {
            DoApplyRewards(progressionInfo);
        }
    }

    public static bool CanApplyRewards(ProgressionInfo progressionInfo)
    {
        return !ProgressionStore.instance.GetProgressionComplete(progressionInfo) && AreConditionsMet(progressionInfo);
    }

    public static void DoApplyRewards(ProgressionInfo progressionInfo)
    {
        ProgressionStore.instance.SetProgressionComplete(progressionInfo, true);

        CurrencySystem.instance.ProgressionReward(progressionInfo);
        MachineInfo[] machineRewards = progressionInfo.machineRewards;
        for (int i = 0, len = machineRewards.Length; i < len; ++i)
        {
            MachineInfo machineReward = machineRewards[i];
            MachineUnlockSystem.instance.Unlock(machineReward);
        }
        MachineGroupInfo[] machineGroupRewards = progressionInfo.machineGroupRewards;
        for (int i = 0, len = machineGroupRewards.Length; i < len; ++i)
        {
            MachineInfo[] machineGroupReward = machineGroupRewards[i].members;
            for (int j = 0, jlen = machineGroupReward.Length; j < jlen; ++j)
            {
                MachineUnlockSystem.instance.Unlock(machineGroupReward[j]);
            }
        }
    }

    public static bool AreConditionsMet(ProgressionInfo progressionInfo)
    {
        if (MachineUnlockConditionsMet(progressionInfo.machineUnlockConditions))
        {
            long levelCondition = progressionInfo.levelCondition;
            CurrencySystem currencySystem = CurrencySystem.instance;
            if (levelCondition == 0 || currencySystem.save.level >= levelCondition)
            {
                long moneyCondition = progressionInfo.moneyCondition;
                if (moneyCondition == 0 || currencySystem.save.money >= moneyCondition)
                {
                    return true;
                }
            }
        }
        return false;
    }

    static bool MachineUnlockConditionsMet(MachineInfo[] machineInfos)
    {
        for (int i = 0, len = machineInfos.Length; i < len; ++i)
        {
            if (!MachineUnlockSystem.instance.unlocked.Contains(machineInfos[i]))
            {
                return false;
            }
        }
        return true;
    }
}
