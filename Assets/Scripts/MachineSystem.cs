using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MachineSystem : Singleton<MachineSystem>
{
    public int queryMaxResults = 24;
    public AudioClip createMachineClip;
    public AudioClip demolishMachineClip;

    [NonSerialized]
    public Dictionary<Collider, Machine> machineColliders = new Dictionary<Collider, Machine>();
    private Collider[] oneColliderBuffer = new Collider[1];
    private Collider[] manyColliderBuffer;
    private Machine[] manyMachinesBuffer;

    protected override void Awake()
    {
        base.Awake();
        manyColliderBuffer = new Collider[queryMaxResults];
        manyMachinesBuffer = new Machine[queryMaxResults];
    }

    public void Add(Machine machine)
    {
        Collider[] colliders = machine.colliders;
        for (int i = 0, len = colliders.Length; i < len; i++)
        {
            machineColliders.Add(colliders[i], machine);
        }
    }

    public void Remove(Machine machine)
    {
        Collider[] colliders = machine.colliders;
        for (int i = 0, len = colliders.Length; i < len; i++)
        {
            bool removed = machineColliders.Remove(colliders[i]);
            Assert.IsTrue(removed);
        }
    }

    public bool MachineExists(Vector3Int position)
    {
        if (Physics.OverlapBoxNonAlloc(position.RoundToTileCenter(), new Vector3(0.5f, 0.5f, 0.5f), oneColliderBuffer) > 0)
        {
            return true;
        }
        return false;
    }

    public bool GetMachine(Vector3Int position, out Machine machine)
    {
        Vector3 center= position.RoundToTileCenter();
        center.y += 0.5f;
        if (Physics.OverlapBoxNonAlloc(center, new Vector3(0.5f, 0.5f, 0.5f), oneColliderBuffer, Quaternion.identity, Layer.GetMask(Layer.machines).value) > 0)
        {
            if (machineColliders.TryGetValue(oneColliderBuffer[0], out machine))
            {
                return true;
            }
            WarnOrphanCollider();
            return false;
        }
        machine = null;
        return false;
    }

    // param machines is reused, don't hold on to it
    public int GetMachines(Bounds3Int position, out Machine[] machinesBuffer)
    {
        machinesBuffer = manyMachinesBuffer;
        int count = Physics.OverlapBoxNonAlloc(position.center, position.size * 0.5f, manyColliderBuffer, Quaternion.identity, Layer.GetMask(Layer.machines).value);
        int machinesCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (machineColliders.TryGetValue(manyColliderBuffer[i], out Machine machine))
            {
                for (int j = 0; j < machinesCount; j++)
                {
                    if (machinesBuffer[j] == machine)
                    {
                        goto skipAddingMachines;
                    }
                }

                machinesBuffer[machinesCount] = machine;
                machinesCount++;
            skipAddingMachines:;
            }
            else
            {
                WarnOrphanCollider();
            }
        }
        return count;
    }

    void WarnOrphanCollider()
    {
        Debug.LogWarning("Collider in Machines layer mask was not found in collider-machine dictionary. Collider has wrong layer mask.");
    }
}
