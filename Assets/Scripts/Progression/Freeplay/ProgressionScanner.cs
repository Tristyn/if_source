using System.Collections.Generic;
using UnityEngine.Rendering;

public class ProgressionScanner
{
    Dictionary<MachineInfo, List<ProgressionInfo>> machineConditions = new Dictionary<MachineInfo, List<ProgressionInfo>>();
    OpenSortedList<long, List<ProgressionInfo>> moneyConditions = new OpenSortedList<long, List<ProgressionInfo>>();
    OpenSortedList<long, List<ProgressionInfo>> levelConditions = new OpenSortedList<long, List<ProgressionInfo>>();

    public void Configure()
    {
        ProgressionInfo[] progressionInfos = ScriptableObjects.instance.progressionInfos;

        machineConditions.Clear();
        Dictionary<long, List<ProgressionInfo>> moneyConditions = new Dictionary<long, List<ProgressionInfo>>();
        Dictionary<long, List<ProgressionInfo>> levelConditions = new Dictionary<long, List<ProgressionInfo>>();

        for (int i = 0, len = progressionInfos.Length; i < len; ++i)
        {
            ProgressionInfo progression = progressionInfos[i];

            // Machine Conditions
            MachineInfo[] machineUnlockConditions = progression.machineUnlockConditions;
            for (int j = 0, jlen = machineUnlockConditions.Length; j < jlen; ++j)
            {
                MachineInfo machineUnlock = machineUnlockConditions[j];
                if (!machineConditions.TryGetValue(machineUnlock, out List<ProgressionInfo> conditions))
                {
                    conditions = new List<ProgressionInfo>();
                    machineConditions.Add(machineUnlock, conditions);
                }
                conditions.Add(progression);
            }

            // Money Conditions
            if (progression.moneyCondition != 0)
            {
                if (!moneyConditions.TryGetValue(progression.moneyCondition, out List<ProgressionInfo> conditions))
                {
                    conditions = new List<ProgressionInfo>();
                    moneyConditions.Add(progression.moneyCondition, conditions);
                }
                conditions.Add(progression);
            }

            // Level Conditions
            if (progression.levelCondition != 0)
            {
                if (!levelConditions.TryGetValue(progression.levelCondition, out List<ProgressionInfo> conditions))
                {
                    conditions = new List<ProgressionInfo>();
                    levelConditions.Add(progression.levelCondition, conditions);
                }
                conditions.Add(progression);
            }
        }

        this.moneyConditions = new OpenSortedList<long, List<ProgressionInfo>>(moneyConditions);
        this.levelConditions = new OpenSortedList<long, List<ProgressionInfo>>(levelConditions);
    }

    public List<ProgressionInfo> RequiringUnlockedMachine(MachineInfo machineInfo)
    {
        if (machineConditions.TryGetValue(machineInfo, out List<ProgressionInfo> progressions))
        {
            return progressions;
        }
        return ListPool<ProgressionInfo>.Get();
    }

    public List<ProgressionInfo> RequiringMoneyInRange(long rangeLowExclusive, long rangeHighInclusive)
    {
        return RequiringValueInRange(moneyConditions, rangeLowExclusive, rangeHighInclusive);
    }

    public List<ProgressionInfo> RequiringLevelInRange(long rangeLowExclusive, long rangeHighInclusive)
    {
        return RequiringValueInRange(levelConditions, rangeLowExclusive, rangeHighInclusive);
    }

    /// <summary>
    /// Result list can be returned to the list pool
    /// </summary>
    /// <param name="sortedProgressions"></param>
    /// <param name="rangeLowExclusive"></param>
    /// <param name="rangeHighInclusive"></param>
    List<ProgressionInfo> RequiringValueInRange(OpenSortedList<long, List<ProgressionInfo>> sortedProgressions, long rangeLowExclusive, long rangeHighInclusive)
    {
        List<ProgressionInfo> list = ListPool<ProgressionInfo>.Get();

        List<ProgressionInfo>[] progressionsList = sortedProgressions.values;
        if (progressionsList.Length == 0)
        {
            return list;
        }
        int start = sortedProgressions.IndexOfKeyGreaterThanOrEqualTo(rangeLowExclusive);
        int end = sortedProgressions.IndexOfKeyLessThanOrEqualTo(rangeHighInclusive);
        if (start == progressionsList.Length)
        {
            if (end == start - 1)
            {
                return list;
            }
            else
            {
                --start;
            }
        }

        for (int i = start; i <= end; ++i)
        {
            list.AddList(progressionsList[i]);
        }

        return list;
    }
}
