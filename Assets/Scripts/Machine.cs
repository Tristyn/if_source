using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public struct MachineConveyorLink
{
    public Conveyor innerConveyor;
    public Conveyor outerConveyor;
    public Directions direction;
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
    public ItemInfo itemInfo;
    public Inventory inventory = Inventory.empty;

    public MachinePurchaser machinePurchaser;
    public MachineSeller machineSeller;
    public MachineAssembler machineAssembler;
    public MachinePlacer machinePlacer;

    public GameObject instance;

    public void Initialize()
    {
        if (machineInfo.prefab)
        {
            instance = Instantiate(machineInfo.prefab, transform);
        }
        else
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.transform.SetParent(transform, false);
            Destroy(instance.GetComponent<BoxCollider>());
            Transform instanceTransform = instance.transform;
            instanceTransform.localPosition = (Vector3)machineInfo.size * 0.5f;
            instanceTransform.localScale = machineInfo.size;
        }

        gameObject.SetAllLayers(Layer.machines);

        machineInfo.inventory.Clone(out inventory);

        if (machineInfo.purchaseItem.itemInfo != null)
        {
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
            machineSeller = gameObject.AddComponent<MachineSeller>();
            machineSeller.machine = this;
        }
        if (machineInfo.assembler)
        {
            machineAssembler = gameObject.AddComponent<MachineAssembler>();
            machineAssembler.machine = this;
            if (!machinePlacer)
            {
                machinePlacer = gameObject.AddComponent<MachinePlacer>();
                machinePlacer.machine = this;
            }
        }

        Color color = machineInfo.color;
        if (color != Color.white)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                Material[] materials = renderers[i].materials;
                for (int j = 0, lenJ = materials.Length; j < lenJ; j++)
                {
                    materials[j].color = color;
                }
            }
        }

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.center = bounds.size * 0.5f;
        // Shrink 0.1f so it doesn't collide with things on the very edge of the tile
        collider.size = bounds.size.Subtract(0.1f);
        colliders = new[] { collider };

        FindConveyors();

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

        MachineSystem.instance.Add(this);
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

    public static Machine CreateMachine(MachineInfo machineInfo, Vector3 center)
    {
        Vector3Int boundsMin = center.PositionToBounds(machineInfo.size).min;
        boundsMin.y = Mathf.FloorToInt(center.y);
        Bounds3Int bounds = new Bounds3Int(boundsMin, boundsMin + machineInfo.size - Vector3Int.one);
        if (MachineSystem.instance.GetMachines(bounds, out Machine[] machines) > 0)
        {
            return null;
        }

        GameObject gameObject = new GameObject(machineInfo.machineName);
        gameObject.transform.position = bounds.min;
        Machine machine = gameObject.AddComponent<Machine>();
        machine.bounds = bounds;
        machine.machineInfo = machineInfo;
        machine.Initialize();
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

        this.conveyors = conveyors.ToArray();
        List<MachineConveyorLink> conveyorLinks = new List<MachineConveyorLink>();

        foreach ((Vector3Int outerTile, Vector3Int innerTile, Directions direction) in bounds.EnumeratePerimeterClockwise())
        {
            if (conveyorDict.TryGetValue(innerTile, out Conveyor innerConveyor))
            {
                MachineConveyorLink link = new MachineConveyorLink
                {
                    innerConveyor = innerConveyor,
                    direction = direction
                };
                Conveyor output = innerConveyor.outputs[(int)direction];
                if (output)
                {
                    link.outerConveyor = output;
                    link.isOutput = true;
                    conveyorLinks.Add(link);
                    continue;
                }
                else
                {
                    Conveyor input = innerConveyor.inputs[(int)direction];
                    if (input)
                    {
                        link.outerConveyor = input;
                        link.isOutput = false;
                        conveyorLinks.Add(link);
                    }
                }
            }
        }

        this.conveyorLinks = conveyorLinks.ToArray();
    }
}
