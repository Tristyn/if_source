using GameAnalyticsSDK;
using UnityEngine.Assertions;

public enum CurrencyEventType : byte
{
    ConveyorPurchased,
    ConveyorSold,
    MachinePurchased,
    MachineSold,
    MachineSellerRevenue,
    MachinePurchaserFees,
    ItemRefunded,
    ProgressionReward,
    ItemPurchased,
    ItemSold,
}

public struct CurrencyEvent
{
    public GAResourceFlowType flowType;
    public CurrencyType currencyType;
    public CurrencyEventType currencyEventType;
    public string eventItemId;

    public CurrencyEvent(GAResourceFlowType flowType, CurrencyType currencyType, CurrencyEventType currencyEventType, string eventItemId)
    {
        Assert.IsNotNull(eventItemId);
        Assert.IsTrue(flowType != GAResourceFlowType.Undefined);
        this.flowType = flowType;
        this.currencyType = currencyType;
        this.currencyEventType = currencyEventType;
        this.eventItemId = eventItemId;
    }

    public CurrencyEvent(GAResourceFlowType flowType, CurrencyType currencyType, CurrencyEventType currencyEventType, ItemInfo itemInfo)
    {
        Assert.IsNotNull(itemInfo);
        Assert.IsTrue(flowType != GAResourceFlowType.Undefined);
        this.flowType = flowType;
        this.currencyType = currencyType;
        this.currencyEventType = currencyEventType;
        eventItemId = itemInfo.itemName;
    }

    public CurrencyEvent(GAResourceFlowType flowType, CurrencyType currencyType, CurrencyEventType currencyEventType, MachineInfo machineInfo)
    {
        Assert.IsNotNull(machineInfo);
        Assert.IsTrue(flowType != GAResourceFlowType.Undefined);
        this.flowType = flowType;
        this.currencyType = currencyType;
        this.currencyEventType = currencyEventType;
        eventItemId = machineInfo.machineName;
    }

    public CurrencyEvent(GAResourceFlowType flowType, CurrencyType currencyType, CurrencyEventType currencyEventType, ProgressionInfo progressionInfo)
    {
        Assert.IsNotNull(progressionInfo);
        Assert.IsTrue(flowType != GAResourceFlowType.Undefined);
        this.flowType = flowType;
        this.currencyType = currencyType;
        this.currencyEventType = currencyEventType;
        eventItemId = progressionInfo.progressionName;
    }

    public override bool Equals(object obj)
    {
        return obj is CurrencyEvent c && this == c;
    }
    public static bool operator ==(CurrencyEvent a, CurrencyEvent b)
    {
        return a.flowType == b.flowType
            && a.currencyType == b.currencyType
            && a.currencyEventType == b.currencyEventType
            && a.eventItemId == b.eventItemId;
    }
    public static bool operator !=(CurrencyEvent a, CurrencyEvent b)
    {
        return a.flowType != b.flowType
            || a.currencyType != b.currencyType
            || a.currencyEventType != b.currencyEventType
            || a.eventItemId != b.eventItemId;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int)2166136261;
            // Suitable nullity checks etc, of course :)
            hash = (hash * 16777619) ^ flowType.GetHashCode();
            hash = (hash * 16777619) ^ currencyType.GetHashCode();
            hash = (hash * 16777619) ^ currencyEventType.GetHashCode();
            hash = (hash * 16777619) ^ eventItemId.GetHashCode();
            return hash;
        }
    }
}
