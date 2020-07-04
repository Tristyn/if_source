using UnityEngine;

public class MachineAssembler : MonoBehaviour
{
    public Machine machine;

    AssembleSlot[] inputs;
    AssembleSlot output;
    Inventory inventory;
    float placeInterval;
    float nextAssembleTime = -1f;

    public void Initialize()
    {
        placeInterval = machine.machineInfo.placeInterval;
        inputs = machine.machineInfo.assembleInputs;
        output = machine.machineInfo.assembleOutput;
        inventory = machine.inventory;
    }

    private void FixedUpdate()
    {
        if (nextAssembleTime <= Time.fixedTime)
        {
            nextAssembleTime += placeInterval;
            if (inventory.HasItems(inputs))
            {
                if (inventory.TryAdd(output.itemInfo, output.count))
                {
                    inventory.DeductItems(inputs);
                }
            }
        }
    }
}
