using Newtonsoft.Json.Utilities;

public static class AotMethods
{
    public static void Ensure()
    {
        AotHelper.EnsureList<Conveyor.Save>();
        AotHelper.EnsureList<ConveyorItem.Save>();
        AotHelper.EnsureList<ConveyorItem.Save[]>();
        AotHelper.EnsureList<Machine.Save>();
        AotHelper.EnsureList<InventorySlot.Save>();
        AotHelper.EnsureList<SpacePlatform.Save>();
        AotHelper.EnsureList<long>();
        AotHelper.EnsureList<Bounds3Int>();
    }
}
