using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public struct MachineConveyorLink
{
    public Conveyor innerConveyor;
    public Conveyor outerConveyor;
    public Directions machineDirection;
    public bool isOutput;
}

public class Machine : MonoBehaviour
{
    public Bounds3Int bounds;
    [ReadOnly(true)]
    public Collider[] colliders;
    [NonSerialized]
    public Conveyor[] conveyors;
    public MachineConveyorLink[] conveyorLinks;
    [NonSerialized]
    public MachineInfo machineInfo;
    [NonSerialized]
    public Inventory inventory = Inventory.empty;

    public bool canOutput;
    public bool canInput;
    public MachinePurchaser machinePurchaser;
    public MachineSeller machineSeller;
    public MachineAssembler machineAssembler;
    public MachinePlacer machinePlacer;
    public MachineVisual instance;

    public void Initialize()
    {
        if (machineInfo.prefab)
        {
            instance = Instantiate(machineInfo.prefab, transform);
        }
        else
        {
            GameObject instanceGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance =  instanceGameObject.AddComponent<MachineVisual>();
            instance.transform.SetParent(transform, false);
            Destroy(instance.GetComponent<BoxCollider>());
            Transform instanceTransform = instance.transform;
            instanceTransform.localPosition = (Vector3)machineInfo.size * 0.5f;
            instanceTransform.localScale = machineInfo.size;
        }

        MachineDropper machineDropper = ObjectPooler.instance.Get<MachineDropper>();
        machineDropper.recycleComponentAfterDrop = true;
        machineDropper.Drop(bounds, instance.transform);

        gameObject.SetAllLayers(Layer.machines);

        inventory = machineInfo.inventory.Clone();

        if (machineInfo.purchaseItem.itemInfo != null)
        {
            canOutput = true;
            machinePurchaser = gameObject.AddComponent<MachinePurchaser>();
            machinePurchaser.machine = this;
            if (!machinePlacer)
            {
                machinePlacer = gameObject.AddComponent<MachinePlacer>();
                machinePlacer.machine = this;
            }
        }
        if (machineInfo.sellItem.itemInfo != null)
        {
            canInput = true;
            machineSeller = gameObject.AddComponent<MachineSeller>();
            machineSeller.machine = this;
        }
        if (machineInfo.assembler)
        {
            canInput = true;
            canOutput = true;
            machineAssembler = gameObject.AddComponent<MachineAssembler>();
            machineAssembler.machine = this;
            if (!machinePlacer)
            {
                machinePlacer = gameObject.AddComponent<MachinePlacer>();
                machinePlacer.machine = this;
            }
        }

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.center = bounds.size * 0.5f;
        // Shrink 0.1f so it doesn't collide with things on the very edge of the tile
        collider.size = bounds.size.Subtract(0.1f);
        colliders = new[] { collider };

        MachineSystem.instance.Add(this);

        if (machinePurchaser)
        {
            machinePurchaser.Initialize();
        }
        if (machineSeller)
        {
            machineSeller.Initialize();
        }
        if (machineAssembler)
        {
            machineAssembler.Initialize();
        }
        if (machinePlacer)
        {
            machinePlacer.Initialize();
        }

        Dictionary<Vector3Int, Conveyor> conveyors = ConveyorSystem.instance.conveyors;
        for (int x = bounds.min.x, lenx = bounds.max.x + 1; x < lenx; x++)
        {
            for (int y = bounds.min.y, leny = bounds.max.y + 1; y < leny; y++)
            {
                for (int z = bounds.min.z, lenz = bounds.max.z + 1; z < lenz; z++)
                {
                    if (conveyors.TryGetValue(new Vector3Int(x, y, z), out Conveyor conveyor))
                    {
                        Assert.IsNull(conveyor.machine);
                        conveyor.machine = this;
                    }
                }
            }
        }

        FindConveyors();
        RecycleInvalidConveyors();
    }

    public void Delete()
    {
        Conveyor[] conv = (Conveyor[])conveyors.Clone();
        for (int i = 0, len = conv.Length; i < len; i++)
        {
            conv[i].Recycle();
        }

        MachineSystem.instance.Remove(this);

        Destroy(gameObject);
    }

    public void PlayDemolishAudio()
    {
        AudioSystem.instance.PlayOneShot(MachineSystem.instance.demolishMachineClip, AudioCategory.Effect);
    }

    public static Machine CreateMachine(MachineInfo machineInfo, Vector3 center)
    {
        Bounds3Int bounds = center.PositionBottomToBounds(machineInfo.size);
        if (MachineSystem.instance.GetMachines(bounds, out Machine[] machines) > 0)
        {
            return null;
        }

        GameObject gameObject = new GameObject(machineInfo.machineName);
        gameObject.transform.localPosition = bounds.min;
        Machine machine = gameObject.AddComponent<Machine>();
        machine.bounds = bounds;
        machine.machineInfo = machineInfo;
        machine.Initialize();

        AudioSystem.instance.PlayOneShot(MachineSystem.instance.createMachineClip, AudioCategory.Effect);

        return machine;
    }

    public void FindConveyors()
    {
        Dictionary<Vector3Int, Conveyor> conveyorDict = ConveyorSystem.instance.conveyors;
        List<Conveyor> conveyors = new List<Conveyor>();

        for (int x = bounds.min.x, lenx = bounds.max.x + 1; x < lenx; x++)
        {
            for (int z = bounds.min.z, lenz = bounds.max.z + 1; z < lenz; z++)
            {
                if (conveyorDict.TryGetValue(new Vector3Int(x, bounds.min.y, z), out Conveyor conveyor))
                {
                    conveyors.Add(conveyor);
                }
            }
        }

        if (conveyors.Count == 0)
        {
            this.conveyors = Array.Empty<Conveyor>();
            this.conveyorLinks = Array.Empty<MachineConveyorLink>();
            return;
        }

        List<MachineConveyorLink> conveyorLinks = new List<MachineConveyorLink>();

        foreach ((Vector3Int outerTile, Vector3Int innerTile, Directions direction) in bounds.EnumeratePerimeterClockwise())
        {
            if (conveyorDict.TryGetValue(innerTile, out Conveyor innerConveyor) &&
                conveyorDict.TryGetValue(outerTile, out Conveyor outerConveyor))
            {
                MachineConveyorLink link = new MachineConveyorLink
                {
                    innerConveyor = innerConveyor,
                    outerConveyor = outerConveyor,
                    machineDirection = direction
                };
                if (innerConveyor.IsLinked(outerConveyor))
                {
                    link.isOutput = true;
                    conveyorLinks.Add(link);
                }
                else if (outerConveyor.IsLinked(innerConveyor))
                {
                    link.isOutput = false;
                    conveyorLinks.Add(link);
                }
            }
        }

        this.conveyors = conveyors.ToArray();
        this.conveyorLinks = conveyorLinks.ToArray();
    }

    /// <summary>
    /// Recycles conveyors that are inside the machine in invalid formations. Returns if any are unlinked, recycled or otherwise modified.
    /// </summary>
    public bool RecycleInvalidConveyors()
    {
        bool conveyorsRecycled = false;
        for (int i = conveyors.Length - 1; i >= 0; i--)
        {
            Conveyor conveyor = conveyors[i];
            if (conveyor)
            {
                Conveyor[] inputs = conveyor.inputs;
                Conveyor[] outputs = conveyor.outputs;
                for (int j = 1, lenj = inputs.Length; j < lenj; j++)
                {
                    Conveyor output = outputs[j];
                    if (output)
                    {
                        if (!ConveyorSystem.instance.CanLink(conveyor.position_local, output.position_local))
                        {
                            conveyorsRecycled = true;
                            conveyor.Unlink(output);
                            if (!output.IsLinked() && output.machine != this)
                            {
                                Assert.IsNotNull(output.machine);
                                output.Recycle();
                            }
                        }
                    }
                    else
                    {
                        Conveyor input = inputs[j];
                        if (input)
                        {
                            if (!ConveyorSystem.instance.CanLink(input.position_local, conveyor.position_local))
                            {
                                conveyorsRecycled = true;
                                input.Unlink(conveyor);
                                if (!input.IsLinked() && input.machine != this)
                                {
                                    Assert.IsNotNull(input.machine);
                                    input.Recycle();
                                }
                            }
                        }
                    }
                }
                if (!conveyor.IsLinked())
                {
                    conveyorsRecycled = true;
                    conveyor.Recycle();
                }
            }
        }
        return conveyorsRecycled;
    }
}
