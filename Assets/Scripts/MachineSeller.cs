using UnityEngine;

public class MachineSeller : MonoBehaviour
{
    public Machine machine;

    AssembleSlot sellItem;
    Inventory inventory;
    float placeInterval;
    float nextAssembleTime = -1f;

    public void Initialize()
    {
        placeInterval = machine.machineInfo.placeInterval;
        sellItem = machine.machineInfo.sellItem;
        inventory = machine.inventory;
    }

    private void FixedUpdate()
    {
        if (nextAssembleTime <= Time.fixedTime)
        {
            nextAssembleTime += placeInterval;
            if (inventory.HasItem(sellItem))
            {
                inventory.DeductItem(sellItem);
                CurrencySystem.instance.ItemSold(sellItem.itemInfo, sellItem.count, machine.bounds.topCenter);
            }
        }
    }
}
