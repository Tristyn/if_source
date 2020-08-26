using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineAssembler : MonoBehaviour
{
    public Machine machine;

    AssembleSlot[] inputs;
    AssembleSlot output;
    Inventory inventory;
    float placeInterval;

    [Serializable]
    public struct Save
    {
        public float nextAssembleTime;
        public long numAssembled;
    }
    [NonSerialized]
    public Save save;

    public void Initialize()
    {
        save.nextAssembleTime = GameTime.fixedTime;
        placeInterval = machine.machineInfo.placeInterval;
        inputs = machine.machineInfo.assembleInputs;
        output = machine.machineInfo.assembleOutput;
        inventory = machine.inventory;
    }

    private void FixedUpdate()
    {
        if (save.nextAssembleTime <= GameTime.fixedTime)
        {
            save.nextAssembleTime += placeInterval;
            if (inventory.HasItems(inputs))
            {
                ref InventorySlot outputSlot = ref inventory.GetSlot(output.itemInfo);
                Assert.IsTrue(outputSlot.valid);
                if (outputSlot.TryAdd(output.count))
                {
                    inventory.DeductItems(inputs);
                    save.numAssembled++;
                }
            }
        }
    }
}
