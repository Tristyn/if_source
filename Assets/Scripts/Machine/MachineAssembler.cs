using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineAssembler : MonoBehaviour, IFixedUpdate
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
        Entities.machineAssemblers.Add(this);
    }

    public void Delete()
    {
        Entities.machineAssemblers.Remove(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        if (save.nextAssembleTime <= GameTime.fixedTime)
        {
            save.nextAssembleTime += placeInterval;

            bool operated = false;

            if (inventory.HasItems(inputs))
            {
                ref InventorySlot outputSlot = ref inventory.GetSlot(output.itemInfo);
                Assert.IsTrue(outputSlot.valid);
                if (outputSlot.TryAdd(output.count))
                {
                    operated = true;
                    inventory.DeductItems(inputs);
                    save.numAssembled++;
                }
            }

            machine.machineEfficiency.Tick(operated);
        }
    }
}
