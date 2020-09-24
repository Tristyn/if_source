using System.Collections.Generic;
using UnityEngine;

public class ProgressionSystem : Singleton<ProgressionSystem>
{
    struct MachineCondition
    {
        public ProgressionInfo progressionInfo;
        public int machineUnlocksIndex;
    }

    public struct Save
    {
        public int[] completedProgressionIds;
    }

    Dictionary<MachineInfo, MachineCondition[]> machineConditions = new Dictionary<MachineInfo, MachineCondition[]>();
    HashSet<int> completedProgressionIds = new HashSet<int>();


    long lastMoney;
    OpenSortedList<long, ProgressionInfo[]> moneyConditions = new OpenSortedList<long, ProgressionInfo[]>();
    long lastLevel;
    OpenSortedList<long, ProgressionInfo[]> levelConditions = new OpenSortedList<long, ProgressionInfo[]>();


    protected override void Awake()
    {
        base.Awake();

        Init.Bind += Bind;
        Init.PostLoad += PostLoad;
        Init.LoadComplete += LoadComplete;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Init.PostLoad -= PostLoad;
        Init.LoadComplete -= LoadComplete;
        if (CurrencySystem.instance)
        {
            CurrencySystem.instance.moneyChanged.RemoveListener(OnMoneyChanged);
            CurrencySystem.instance.levelChanged.RemoveListener(OnLevelChanged);
        }
    }


    void Bind()
    {
        CurrencySystem.instance.moneyChanged.AddListener(OnMoneyChanged);
        CurrencySystem.instance.levelChanged.AddListener(OnLevelChanged);

        Dictionary<MachineInfo, List<MachineCondition>> machineConditions = new Dictionary<MachineInfo, List<MachineCondition>>();
        ProgressionInfo[] progressionInfos = ScriptableObjects.instance.progressionInfos;
        for (int i = 0, len = progressionInfos.Length; i < len; ++i)
        {
            ProgressionInfo progression = progressionInfos[i];
            MachineInfo[] machineUnlockConditions = progression.machineUnlockConditions;
            for (int j = 0, jlen = machineUnlockConditions.Length; j < jlen; ++j)
            {
                MachineInfo machineUnlock = machineUnlockConditions[j];
                if (!machineConditions.TryGetValue(machineUnlock, out List<MachineCondition> conditions))
                {
                    conditions = new List<MachineCondition>();
                    machineConditions.Add(machineUnlock, conditions);
                }
                conditions.Add(new MachineCondition
                {
                    progressionInfo = progression,
                    machineUnlocksIndex = j
                });
            }
        }

        foreach (KeyValuePair<MachineInfo, List<MachineCondition>> machineCondition in machineConditions)
        {
            this.machineConditions.Add(machineCondition.Key, machineCondition.Value.ToArray());
        }
    }

    public void GetSave(out Save save)
    {
        int[] completedProgressionIds = new int[this.completedProgressionIds.Count];
        save.completedProgressionIds = completedProgressionIds;
        int i = 0;
        foreach (var id in this.completedProgressionIds)
        {
            completedProgressionIds[i] = id;
            i++;
        }
    }

    public void SetSave(in Save save)
    {
        completedProgressionIds.Clear();
        if (save.completedProgressionIds != null)
        {
            for (int i = 0, len = save.completedProgressionIds.Length; i < len; ++i)
            {
                if (!completedProgressionIds.Contains(save.completedProgressionIds[i]))
                {
                    completedProgressionIds.Add(save.completedProgressionIds[i]);
                }
                else
                {
                    Debug.LogWarning("Found progression id more than once in save file. " + save.completedProgressionIds[i]);
                }
            }
        }
    }

    void PostLoad()
    {
        lastMoney = 0;
        lastLevel = 0;
    }

    void LoadComplete()
    {
        HashSet<MachineInfo> unlocked = MachineUnlockSystem.instance.unlocked;
        ProgressionInfo[] progressionInfos = ScriptableObjects.instance.progressionInfos;
        for (int i = 0, len = progressionInfos.Length; i < len; ++i)
        {
            ProgressionInfo progressionInfo = progressionInfos[i];
            BitArray32 machineUnlockConditionsMet = new BitArray32();
            MachineInfo[] machines = progressionInfo.machineUnlockConditions;
            for (int j = 0, lenj = machines.Length; j < lenj; ++j)
            {
                if (unlocked.Contains(machines[j]))
                {
                    machineUnlockConditionsMet[j] = true;
                }
            }
            progressionInfo.machineUnlockConditionsMet = machineUnlockConditionsMet;
            if (!completedProgressionIds.Contains(progressionInfo.progressionId))
            {
                TestConditionsMet(progressionInfo);
            }
        }
    }

    public void OnMachineUnlocked(MachineInfo machineInfo)
    {
        if (machineConditions.TryGetValue(machineInfo, out MachineCondition[] conditions))
        {
            for (int i = 0, len = conditions.Length; i < len; ++i)
            {
                MachineCondition condition = conditions[i];
                condition.progressionInfo.machineUnlockConditionsMet[condition.machineUnlocksIndex] = true;
                TestConditionsMet(condition.progressionInfo);
            }
        }
    }

    void OnMoneyChanged()
    {
        long money = CurrencySystem.instance.save.money;
        if (lastMoney < money)
        {
            TestRangeOfConditionsMet(moneyConditions, lastMoney, money);
        }
        lastMoney = money;
    }

    void OnLevelChanged()
    {
        long level = CurrencySystem.instance.save.level;
        if (lastLevel < level)
        {
            TestRangeOfConditionsMet(levelConditions, lastLevel, level);
        }
        lastLevel = level;
    }

    void TestRangeOfConditionsMet(OpenSortedList<long, ProgressionInfo[]> sortedProgressions, long oldValue, long newValue)
    {
        ProgressionInfo[][] progressionsList = sortedProgressions.values;
        if (progressionsList.Length == 0)
        {
            return;
        }
        int start = sortedProgressions.IndexOfKeyGreaterThanOrEqualTo(oldValue);
        int end = sortedProgressions.IndexOfKeyLessThanOrEqualTo(newValue);
        if (start == progressionsList.Length)
        {
            if (end == start - 1)
            {
                return;
            }
            else
            {
                --start;
            }
        }

        for (int i = start; i <= end; ++i)
        {
            ProgressionInfo[] progressions = progressionsList[i];
            for (int j = 0, lenj = progressions.Length; j < lenj; ++j)
            {
                TestConditionsMet(progressions[j]);
            }
        }
    }

    void TestConditionsMet(ProgressionInfo progressionInfo)
    {
        if (AreConditionsMet(progressionInfo))
        {
            ApplyConditionRewards(progressionInfo);
        }
    }

    bool AreConditionsMet(ProgressionInfo progressionInfo)
    {
        if (!completedProgressionIds.Contains(progressionInfo.progressionId))
        {
            int lenMachineUnlockConditions = progressionInfo.machineUnlockConditions.Length;
            if (lenMachineUnlockConditions == 0
                || (progressionInfo.machineUnlockConditionsMet & new BitArray32(Mathx.BitCountToMask(lenMachineUnlockConditions))) == progressionInfo.machineUnlockConditionsMet)
            {
                long levelCondition = progressionInfo.levelCondition;
                CurrencySystem currencySystem = CurrencySystem.instance;
                if (levelCondition == 0 || levelCondition >= currencySystem.save.level)
                {
                    long moneyCondition = progressionInfo.moneyCondition;
                    if (moneyCondition == 0 || moneyCondition >= currencySystem.save.money)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void ApplyConditionRewards(ProgressionInfo progressionInfo)
    {
        CurrencySystem.instance.ProgressionReward(progressionInfo);
        MachineInfo[] machineUnlockRewards = progressionInfo.machineUnlockRewards;
        for (int i = 0, len = machineUnlockRewards.Length; i < len; ++i)
        {
            MachineInfo machineUnlock = machineUnlockRewards[i];
            MachineUnlockSystem.instance.Unlock(machineUnlock);
        }
    }
}
