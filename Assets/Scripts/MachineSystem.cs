using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MachineSystem : Singleton<MachineSystem>
{
    public AudioClip createMachineClip;
    public AudioClip demolishMachineClip;

    [NonSerialized]
    public HashSet<Machine> machines = new HashSet<Machine>();
    [NonSerialized]
    public SpatialHash<Machine> machineSpatialHash;

    [Serializable]
    public struct Save
    {
        public Machine.Save[] machines;
    }
    [NonSerialized]
    public Save save;

    protected override void Awake()
    {
        base.Awake();
        machineSpatialHash.Initialize();
        Init.PreSave += PreSave;
        Init.PostSave += PostSave;
        Init.PreLoad += PreLoad;
        Init.PostLoad += PostLoad;
    }

    protected override void OnDestroy()
    {
        Init.PreSave -= PreSave;
        Init.PostSave -= PostSave;
        Init.PreLoad -= PreLoad;
        Init.PostLoad -= PostLoad;
    }

    public void Add(Machine machine)
    {
        Assert.IsFalse(machineSpatialHash.Overlaps(machine.bounds));
        machineSpatialHash.Add(machine, machine.bounds);
        machines.Add(machine);
    }

    public void Remove(Machine machine)
    {
        machines.Remove(machine);
        machineSpatialHash.Remove(machine, in machine.bounds);
    }
    public bool MachineExists(Bounds3Int bounds)
    {
        return machineSpatialHash.Overlaps(bounds);
    }

    public bool MachineExists(Vector3Int tile)
    {
        return machineSpatialHash.Overlaps(tile);
    }

    public Machine GetMachine(Vector3Int tile)
    {
        return machineSpatialHash.GetSingle(tile);
    }

    void PreSave()
    {
        HashSet<Machine> machines = this.machines;
        int i = 0;
        int len = machines.Count;
        Machine.Save[] machineSaves = new Machine.Save[len];
        foreach (Machine machine in machines)
        {
            machine.GetSave(out machineSaves[i]);
            ++i;
        }

        save.machines = machineSaves;
    }

    void PostSave()
    {
        save = default;
    }

    void PreLoad()
    {
        Machine[] machinesClone = new Machine[machines.Count];
        machines.CopyTo(machinesClone);
        for (int i = 0, len = machinesClone.Length; i < len; ++i)
        {
            machinesClone[i].Delete();
        }
    }

    void PostLoad()
    {
        Machine.Save[] saveMachines = save.machines;
        for (int i = 0, len = saveMachines.Length; i < len; ++i)
        {
            ref Machine.Save saveMachine = ref saveMachines[i];
            MachineInfo machineInfo = ScriptableObjects.instance.GetMachineInfo(saveMachine.machineName);
            if (machineInfo)
            {
                Machine machine = Machine.CreateMachine(machineInfo, saveMachine.bounds.bottomCenter);
                machine.SetSave(in saveMachine);
            }
            else
            {
                Debug.LogWarning($"Failed to find MachineInfo {saveMachine.machineName} while loading machine.");
            }

        }
        save = default;
    }

    public void MachineLanded()
    {
        CameraShake.instance.MachineLanded();
    }

    void OnDrawGizmosSelected()
    {
        if (!machineSpatialHash.initialized)
        {
            machineSpatialHash.Initialize();
        }
        foreach (var bucket in machineSpatialHash.buckets)
        {
            Gizmos.color = Color.white;
            var bounds = new Bounds3Int(bucket.Key, bucket.Key.Add(SpatialHash.CELL_SIZE) - Vector3Int.one);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = Color.red;
            foreach (var entry in bucket.Value)
            {
                Gizmos.DrawWireCube(entry.bounds.center, entry.bounds.size - new Vector3(0.2f, 0.2f, 0.2f));
            }
        }
    }
}
