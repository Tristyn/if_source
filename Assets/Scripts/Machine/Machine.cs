using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

[Serializable]
public struct MachineConveyorLink
{
    public Conveyor innerConveyor;
    public Conveyor outerConveyor;
    public Directions machineDirection;
    public bool isOutput;
}

public sealed class Machine : MonoBehaviour
{
    public Bounds3Int bounds;
    [NonSerialized]
    public Conveyor[] conveyors;
    public MachineConveyorLink[] conveyorLinks;
    [NonSerialized]
    public MachineInfo machineInfo;
    public Inventory inventory = Inventory.empty;
	public MachineEfficiency machineEfficiency;
	
    public bool canOutput;
    public bool canInput;
    public MachinePurchaser machinePurchaser;
    public MachineSeller machineSeller;
    public MachineAssembler machineAssembler;
    public MachinePlacer machinePlacer;
    public MachineVisual instance;

    [Serializable]
    public struct Save
    {
        public Bounds3Int bounds;
        public string machineName;
        public Inventory.Save inventory;
        public MachineAssembler.Save machineAssembler;
        public MachinePlacer.Save machinePlacer;
        public MachinePurchaser.Save machinePurchaser;
        public MachineSeller.Save machineSeller;
    }

    public void Initialize()
    {
        transform.localPosition = bounds.min;

        if (machineInfo.prefab)
        {
            instance = Instantiate(machineInfo.prefab, transform);
        }
        else
        {
            GameObject instanceGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance = instanceGameObject.AddComponent<MachineVisual>();
            instance.transform.SetParent(transform, false);
            Destroy(instance.GetComponent<BoxCollider>());
            Transform instanceTransform = instance.transform;
            instanceTransform.localPosition = (Vector3)machineInfo.size * 0.5f;
            instanceTransform.localScale = machineInfo.size;
        }

        gameObject.SetAllLayers(Layer.machines);

        inventory = Inventory.CreateInventory(machineInfo);

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
        for (int x = bounds.min.x, lenx = bounds.max.x; x <= lenx; ++x)
        {
            for (int y = bounds.min.y, leny = bounds.max.y; y <= leny; ++y)
            {
                for (int z = bounds.min.z, lenz = bounds.max.z; z <= lenz; ++z)
                {
                    if (conveyors.TryGetValue(new Vector3Int(x, y, z), out Conveyor conveyor))
                    {
                        conveyor.LinkMachine(this);
                    }
                }
            }
        }

        FindConveyors();
        RecycleInvalidConveyors();
    }

    public void GetSave(out Save save)
    {
        save.bounds = bounds;
        save.machineName = machineInfo.machineName;
        inventory.GetSave(out save.inventory);
        save.machineAssembler = machineAssembler?.save ?? default;
        save.machinePlacer = machinePlacer?.save ?? default;
        save.machinePurchaser = machinePurchaser?.save ?? default;
        save.machineSeller = machineSeller?.save ?? default;
    }

    public void SetSave(in Save save)
    {
        Debug.Assert(bounds == save.bounds);
        Debug.Assert(machineInfo.machineName == save.machineName);
        inventory.SetSave(in save.inventory);
        if (machineAssembler)
        {
            machineAssembler.save = save.machineAssembler;
        }
        if (machinePlacer)
        {
            machinePlacer.save = save.machinePlacer;
        }
        if (machinePurchaser)
        {
            machinePurchaser.save = save.machinePurchaser;
        }
        if (machineSeller)
        {
            machineSeller.save = save.machineSeller;
        }
    }

    public void Demolish()
    {
        CurrencySystem.instance.RefundInventory(inventory);
        MachineGroupAchievements.instance.OnMachineDemolished(machineInfo.machineGroup);
        PlayDemolishAudio();
        Delete();
    }

    public void Delete()
    {
        Events.machineDeleted?.Invoke(this);

        if (machinePurchaser)
        {
            machinePurchaser.Delete();
        }
        if (machineSeller)
        {
            machineSeller.Delete();
        }
        if (machineAssembler)
        {
            machineAssembler.Delete();
        }
        if (machinePlacer)
        {
            machinePlacer.Delete();
        }

        Conveyor[] conv = (Conveyor[])conveyors.Clone();
        for (int i = 0, len = conv.Length; i < len; ++i)
        {
            Assert.IsNotNull(conv[i].machine);
            Assert.IsTrue(conv[i].machine == this);
            conv[i].Recycle();
        }

        Assert.IsTrue(conveyors.Length == 0);
        Assert.IsTrue(conveyorLinks.Length == 0);

        Destroy(gameObject);
    }

    private void PlayDemolishAudio()
    {
        AudioSystem.instance.PlayOneShot(MachineSystem.instance.demolishMachineClip, AudioCategory.Effect);
    }

    public void Drop()
    {
        MachineDropper machineDropper = ObjectPooler.instance.Get<MachineDropper>();
        machineDropper.Drop(bounds, instance.transform);
        AudioSystem.instance.PlayOneShot(MachineSystem.instance.createMachineClip, AudioCategory.Effect);
    }

    public void FindConveyors()
    {
        Dictionary<Vector3Int, Conveyor> conveyorDict = ConveyorSystem.instance.conveyors;
        List<Conveyor> conveyors = new List<Conveyor>();

        for (int x = bounds.min.x, lenx = bounds.max.x + 1; x < lenx; ++x)
        {
            for (int z = bounds.min.z, lenz = bounds.max.z + 1; z < lenz; ++z)
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
        using (ListPool<Conveyor>.Get(out List<Conveyor> toRecycle))
        {
            Assert.IsTrue(toRecycle.Count == 0);
            Directions[] directions = EnumUtil<Directions>.values;
            int directionsLen = directions.Length;

            for (int i = conveyors.Length - 1; i >= 0; i--)
            {
                Conveyor conveyor = conveyors[i];
                if (conveyor)
                {
                    for (int j = 1; j < directionsLen; ++j)
                    {
                        Directions direction = directions[j];
                        Conveyor output = conveyor.TryGetOutput(direction);
                        Conveyor input;
                        if (output)
                        {
                            if (!ConveyorSystem.instance.CanLink(conveyor.save.position_local, output.save.position_local))
                            {
                                conveyor.Unlink(output);

                                if (!output.HasAnyLinks() && output.machine != this && output.machine != null)
                                {
                                    if (!toRecycle.Contains(output))
                                    {
                                        toRecycle.Add(output);
                                    }
                                }
                            }
                        }
                        else if (input = conveyor.TryGetInput(direction))
                        {
                            if (!ConveyorSystem.instance.CanLink(input.save.position_local, conveyor.save.position_local))
                            {
                                input.Unlink(conveyor);
                                if (!input.HasAnyLinks() && input.machine != this && input.machine != null)
                                {
                                    if (!toRecycle.Contains(input))
                                    {
                                        toRecycle.Add(input);
                                    }
                                }
                            }
                        }
                    }

                    if (!conveyor.HasAnyLinks())
                    {
                        if (!toRecycle.Contains(conveyor))
                        {
                            toRecycle.Add(conveyor);
                        }
                    }
                }
            }

            int numRecycled = toRecycle.Count;
            for (int i = 0; i < numRecycled; ++i)
            {
                Conveyor conveyor = toRecycle[i];
                conveyor.Recycle();
            }
            return numRecycled > 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!MachineSystem.instance.machineSpatialHash.initialized)
        {
            MachineSystem.instance.machineSpatialHash.Initialize();
        }

        SpatialHash<Machine> spatialHash = MachineSystem.instance.machineSpatialHash;
        Vector3Int min = spatialHash.GetBucketId(bounds.min);
        Vector3Int max = spatialHash.GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    if (spatialHash.buckets.TryGetValue(new Vector3Int(x, y, z), out var bucket) && bucket.Any(entry => entry.value == this))
                    {
                        Gizmos.color = Color.white;
                        var bucketBounds = Bounds3Int.Create(new Vector3Int(x, y, z), new Vector3Int(SpatialHash.CELL_SIZE, SpatialHash.CELL_SIZE, SpatialHash.CELL_SIZE));
                        Gizmos.DrawWireCube(bucketBounds.center, bucketBounds.size);
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(bounds.center, bounds.size - new Vector3(0.2f, 0.2f, 0.2f));
                    }
                    else
                    {
                        Debug.LogError("Machine missing from spatial hash", this);
                    }
                }
            }
        }
    }
}
