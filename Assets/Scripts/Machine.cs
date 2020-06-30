using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

public class Machine : MonoBehaviour
{
    public Bounds3Int bounds;
    [ReadOnly(true)]
    public Collider[] colliders;
    [NonSerialized]
    public Conveyor[] conveyors;
    [NonSerialized]
    public MachineInfo machineInfo;
    [NonSerialized]
    public ItemInfo itemInfo;

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

        gameObject.SetAllLayers(Layer.machine);

        if (machineInfo.purchaseItem)
        {
            machinePurchaser = gameObject.AddComponent<MachinePurchaser>();
            machinePurchaser.machine = this;
            if (!machinePlacer)
            {
                machinePlacer = gameObject.AddComponent<MachinePlacer>();
                machinePlacer.machine = this;
            }
        }
        if (machineInfo.sellItem)
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
        collider.size = bounds.size;
        colliders = new[] { collider };

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
        for (int i = 0, len = conveyors.Length; i < len; i++)
        {
            Conveyor conveyor = conveyors[i];
            RemoveConveyor(conveyor);
            conveyor.Recycle();
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

    public void AddConveyor(Conveyor conveyor)
    {
        Assert.IsFalse(conveyors.Contains(conveyor));
        Assert.IsTrue(bounds.Contains(conveyor.position));
        Assert.IsTrue(bounds.Perimeter(conveyor.position));
        Assert.IsNull(GetConveyor(conveyor.position));
        conveyors = conveyors.Append(conveyor);
        conveyor.machine = this;
    }

    public void RemoveConveyor(Conveyor conveyor)
    {
        Assert.IsTrue(conveyors.Contains(conveyor));
        Assert.IsTrue(bounds.Contains(conveyor.position));
        Assert.IsTrue(bounds.Perimeter(conveyor.position));
        Assert.IsNotNull(GetConveyor(conveyor.position));
        conveyors.Remove(conveyor);
        conveyor.machine = null;
    }

    public Conveyor GetConveyor(Vector3Int position)
    {
        for (int i = 0, len = conveyors.Length; i < len; i++)
        {
            Conveyor conveyor = conveyors[i];
            if (conveyor.position == position)
            {
                return conveyor;
            }
        }
        return null;
    }

    private void OnValidate()
    {
        colliders = GetComponentsInChildren<Collider>(true);
    }
}
