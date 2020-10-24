using System.Collections.Generic;
using UnityEngine.Rendering;

public sealed class ProgressionSystem : Singleton<ProgressionSystem>
{
    long lastMoney;
    long lastLevel;

    public ProgressionScanner progressionScanner = new ProgressionScanner();
    public List<ProgressionInfo> startingProgressions;

    protected override void Awake()
    {
        base.Awake();

        Init.Configure += Configure;
        Init.Bind += Bind;
        SaveLoad.LoadComplete += LoadComplete;
        Events.MachineUnlocked += OnMachineUnlocked;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        SaveLoad.LoadComplete -= LoadComplete;
        Events.MachineUnlocked -= OnMachineUnlocked;
        if (CurrencySystem.instance)
        {
            CurrencySystem.instance.moneyChanged.RemoveListener(OnMoneyChanged);
            CurrencySystem.instance.levelChanged.RemoveListener(OnLevelChanged);
        }
    }

    void Configure()
    {
        progressionScanner.Configure();
    }

    void Bind()
    {
        CurrencySystem.instance.moneyChanged.AddListener(OnMoneyChanged);
        CurrencySystem.instance.levelChanged.AddListener(OnLevelChanged);
    }

    void LoadComplete()
    {
        lastMoney = CurrencySystem.instance.save.money;
        lastLevel = CurrencySystem.instance.save.level;

        List<ProgressionInfo> progressionInfos = progressionScanner.RequiringMoneyInRange(0, lastMoney);
        ProgressionApplyer.ApplyRewards(progressionInfos);
        ListPool<ProgressionInfo>.Release(progressionInfos);

        progressionInfos = progressionScanner.RequiringLevelInRange(0, lastLevel);
        ProgressionApplyer.ApplyRewards(progressionInfos);
        ListPool<ProgressionInfo>.Release(progressionInfos);

        MachineInfo[] machineInfos = MachineUnlockSystem.instance.unlocked.ToArray();
        for (int i = 0, len = machineInfos.Length; i < len; i++)
        {
            progressionInfos = progressionScanner.RequiringUnlockedMachine(machineInfos[i]);
            ProgressionApplyer.ApplyRewards(progressionInfos);
            ListPool<ProgressionInfo>.Release(progressionInfos);
        }
    }

    void OnMachineUnlocked(MachineInfo machineInfo)
    {
        List<ProgressionInfo> progressionInfos = progressionScanner.RequiringUnlockedMachine(machineInfo);
        ProgressionApplyer.ApplyRewards(progressionInfos);
        ListPool<ProgressionInfo>.Release(progressionInfos);
    }

    void OnMoneyChanged()
    {
        long money = CurrencySystem.instance.save.money;
        if (lastMoney < money)
        {
            List<ProgressionInfo> progressionInfos = progressionScanner.RequiringMoneyInRange(lastMoney, money);
            ProgressionApplyer.ApplyRewards(progressionInfos);
            ListPool<ProgressionInfo>.Release(progressionInfos);
        }
        lastMoney = money;
    }

    void OnLevelChanged()
    {
        long level = CurrencySystem.instance.save.level;
        if (lastLevel < level)
        {
            List<ProgressionInfo> progressionInfos = progressionScanner.RequiringLevelInRange(lastLevel, level);
            ProgressionApplyer.ApplyRewards(progressionInfos);
            ListPool<ProgressionInfo>.Release(progressionInfos);
        }
        lastLevel = level;
    }

    public void ApplyStartingProgressions()
    {
        ProgressionApplyer.ApplyRewards(startingProgressions);
    }
}
