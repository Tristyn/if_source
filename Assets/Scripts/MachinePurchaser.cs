using UnityEngine;


public class MachinePurchaser : MonoBehaviour
{
    public Machine machine;

    Inventory inventory;
    AssembleSlot purchaseItem;

    float placeInterval;
    float nextPlaceTime = -1f;

    public void Initialize()
    {
        inventory = machine.inventory;
        purchaseItem = machine.machineInfo.purchaseItem;
        placeInterval = machine.machineInfo.placeInterval;
    }

    void FixedUpdate()
    {
        if (nextPlaceTime <= Time.fixedTime)
        {
            nextPlaceTime += placeInterval;
            inventory.TryAdd(purchaseItem.itemInfo, purchaseItem.count);
        }
    }
}