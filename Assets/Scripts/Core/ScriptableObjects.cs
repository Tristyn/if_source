public sealed class ScriptableObjects : Singleton<ScriptableObjects>
{
    public ScriptableObjectMasterList masterList;

    protected override void Awake()
    {
        base.Awake();
        masterList.Initialize();
    }

    public ItemInfo GetItemInfo(string name)
    {
        if (masterList.itemsDict.TryGetValue(name, out ItemInfo itemInfo))
        {
            return itemInfo;
        }
        return null;
    }

    public MachineInfo GetMachineInfo(string name)
    {
        if(masterList.machinesDict.TryGetValue(name, out MachineInfo machineInfo))
        {
            return machineInfo;
        }
        return null;
    }
}
