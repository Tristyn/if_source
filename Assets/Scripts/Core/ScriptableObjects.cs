public sealed class ScriptableObjects : Singleton<ScriptableObjects>
{
    public ScriptableObjectMasterList masterList;
    public ItemInfo GetItemInfo(string name)
    {
        if (masterList.allItems.TryGetValue(name, out ItemInfo itemInfo))
        {
            return itemInfo;
        }
        return null;
    }

    public MachineInfo GetMachineInfo(string name)
    {
        if(masterList.allMachines.TryGetValue(name, out MachineInfo machineInfo))
        {
            return machineInfo;
        }
        return null;
    }
}
