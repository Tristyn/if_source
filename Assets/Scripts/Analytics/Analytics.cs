using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum UiEventId
{
    None = 0,
    ButtonRotateLeft = 1,
    ButtonRotateRight = 2,
    ButtonSelectMachine = 3,
    ButtonDemolish = 4,
    ButtonLinkConveyor = 5,
    ButtonLinkConveyorToMachine = 6,
}

public sealed class Analytics : Singleton<Analytics>
{
    public long[] uiEventMilestones;

    // Resource analytics are summed and sent before saving.
    Dictionary<CurrencyEvent, long> currencyEventSums = new Dictionary<CurrencyEvent, long>();
    // UI events are tallied and send before saving if a milestone has been reached since last save
    long[] currentUiEventCounts = new long[EnumUtils<UiEventId>.valuesLength];
    HashSet<(string, MachineGroupAchievementCategory)> queuedMachineGroupProgressionEvents = new HashSet<(string, MachineGroupAchievementCategory)>();

    public struct Save
    {
        public long[] uiEventCounts;
    }
    [NonSerialized]
    public Save save;

    protected override void Awake()
    {
        base.Awake();
        save.uiEventCounts = new long[EnumUtils<UiEventId>.valuesLength];
        Init.Configure += Configure;
        Init.PreSave += PreSave;
        Init.PostLoad += PostLoad;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Init.PreSave -= PreSave;
        Init.PostLoad -= PostLoad;
    }

    void Configure()
    {
        GameAnalytics.Initialize();
    }

    void PreSave()
    {
        SendQueuedEvents();
        ClearQueuedEvents();
    }

    void PostLoad()
    {
        ClearQueuedEvents();
        if (save.uiEventCounts == null)
        {
            save.uiEventCounts = Array.Empty<long>();
        }
        if (EnumUtils<UiEventId>.valuesLength != save.uiEventCounts.Length)
        {
            long[] resizedUiEventCounts = new long[EnumUtils<UiEventId>.valuesLength];
            Array.Copy(save.uiEventCounts, resizedUiEventCounts, Mathf.Min(save.uiEventCounts.Length, resizedUiEventCounts.Length));
            save.uiEventCounts = resizedUiEventCounts;
        }
        currentUiEventCounts = (long[])save.uiEventCounts.Clone();
    }

    void SendQueuedEvents()
    {
        foreach (KeyValuePair<CurrencyEvent, long> entry in currencyEventSums)
        {
            long sum = entry.Value;
            if (sum != 0)
            {
                CurrencyEvent currencyEvent = entry.Key;
                string currencyType = EnumUtils<CurrencyType>.names[(int)currencyEvent.currencyType];
                string currencyEventType = EnumUtils<CurrencyEventType>.names[(int)currencyEvent.currencyEventType];
                GameAnalytics.NewResourceEvent(currencyEvent.flowType, currencyType, sum, currencyEventType, currencyEvent.eventItemId);
            }
        }

        for (int i = 0, len = currentUiEventCounts.Length; i < len; ++i)
        {
            long lastMilestone;
            long? nextMilestone;
            long count = save.uiEventCounts[i];
            do
            {
                (lastMilestone, nextMilestone) = GetUiEventMilestones(count);
                if (nextMilestone.HasValue && currentUiEventCounts[i] >= nextMilestone)
                {
                    count = nextMilestone.Value;
                }
            } while (nextMilestone.HasValue && count >= nextMilestone);
            save.uiEventCounts[i] = currentUiEventCounts[i];
        }

        foreach ((string machineGroupName, MachineGroupAchievementCategory machineGroupProgressionEvent) in queuedMachineGroupProgressionEvents)
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "machine_groups", machineGroupName, EnumUtils<MachineGroupAchievementCategory>.names[(int)machineGroupProgressionEvent]);
        }
    }

    void ClearQueuedEvents()
    {
        currencyEventSums.Clear();
        queuedMachineGroupProgressionEvents.Clear();
    }

    (long lastMilestone, long? nextMilestone) GetUiEventMilestones(long count)
    {
        for (int i = 0, len = uiEventMilestones.Length; i < len; ++i)
        {
            long milestone = uiEventMilestones[i];
            if (count >= milestone)
            {
                if (i + 1 < len)
                {
                    return (milestone, uiEventMilestones[i + 1]);
                }
                else
                {
                    return (milestone, null);
                }
            }
        }
        return (0, null);
    }

    public void NewCurrencyEvent(CurrencyEvent currencyEvent, long amount)
    {
        if (currencyEventSums.TryGetValue(currencyEvent, out long sum))
        {
            sum += amount;
            currencyEventSums[currencyEvent] = sum;
        }
        else
        {
            currencyEventSums.Add(currencyEvent, amount);
        }
    }

    public void NewUiEvent(UiEventId uiEventId, int count)
    {
        currentUiEventCounts[(int)uiEventId] += count;
    }

    public void NewMachineGroupProgressionEvent(MachineGroupInfo machineGroup, MachineGroupAchievementCategory machineGroupProgressionEventId)
    {
        var tuple = (machineGroup.machineGroupName, machineGroupProgressionEventId);
        if (!queuedMachineGroupProgressionEvents.Contains(tuple))
        {
            queuedMachineGroupProgressionEvents.Add(tuple);
        }
    }
}