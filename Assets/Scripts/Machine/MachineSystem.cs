using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct MachineMetaData
{
    public int numInstances;
}

public sealed class MachineSystem : Singleton<MachineSystem>
{
    public AudioClip createMachineClip;
    public AudioClip demolishMachineClip;

    [NonSerialized]
    public HashSet<Machine> machines = new HashSet<Machine>();
    [NonSerialized]
    public SpatialHash<Machine> machineSpatialHash;
    [NonSerialized]
    public Dictionary<MachineInfo, MachineMetaData> machinesMetaData = new Dictionary<MachineInfo, MachineMetaData>();


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

    public bool CanCreateMachine(MachineInfo machineInfo, Bounds3Int bounds)
    {
        return !MachineExists(bounds) && LandSystem.instance.CanBuild(bounds);
    }

    public Machine CreateMachine(MachineInfo machineInfo, Vector3 bottomCenter)
    {
        Bounds3Int bounds = bottomCenter.PositionBottomToBounds(machineInfo.size);
        if (CanCreateMachine(machineInfo, bounds))
        {
            return DoCreateMachine(machineInfo, bounds);
        }
        return null;
    }

    Machine DoCreateMachine(MachineInfo machineInfo, Bounds3Int bounds)
    {
        GameObject gameObject = new GameObject(machineInfo.machineName);
        Machine machine = gameObject.AddComponent<Machine>();
        machine.bounds = bounds;
        machine.machineInfo = machineInfo;
        
        Add(machine);

        machine.Initialize();

        MachineGroupAchievements.instance.OnMachineCreated(machineInfo.machineGroup);

        return machine;
    }

    void Add(Machine machine)
    {
        Assert.IsFalse(machineSpatialHash.Overlaps(machine.bounds));
        machines.Add(machine);
        machineSpatialHash.Add(machine, machine.bounds);
        MachineInfo machineInfo = machine.machineInfo;

        bool machinesMetaDataExists = machinesMetaData.TryGetValue(machineInfo, out MachineMetaData machineMetaData);
        machineMetaData.numInstances++;
        if(machinesMetaDataExists)
        { 
            machinesMetaData[machineInfo] = machineMetaData;
        }
        else
        {
            machinesMetaData.Add(machineInfo, machineMetaData);
        }
    }

    public void Deleted(Machine machine)
    {
        bool exists = machines.Remove(machine);
        Assert.IsTrue(exists);
        machineSpatialHash.Remove(machine, in machine.bounds);
        MachineInfo machineInfo = machine.machineInfo;
        exists = machinesMetaData.TryGetValue(machineInfo, out MachineMetaData machineMetaData);
        Assert.IsTrue(exists);
        machineMetaData.numInstances--;
        if (machineMetaData.numInstances > 0)
        {
            machinesMetaData[machineInfo] = machineMetaData;
        }
        else
        {
            exists = machinesMetaData.Remove(machineInfo);
            Assert.IsTrue(exists);
        }
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
        return machineSpatialHash.GetFirst(tile);
    }

    void PreSave()
    {
        HashSet<Machine> machines = this.machines;
        Machine.Save[] machineSaves = new Machine.Save[machines.Count];
        int i = 0;
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
        Machine.Save[] saveMachines = save.machines ?? Array.Empty<Machine.Save>();
        for (int i = 0, len = saveMachines.Length; i < len; ++i)
        {
            ref Machine.Save saveMachine = ref saveMachines[i];
            MachineInfo machineInfo = ScriptableObjects.instance.GetMachineInfo(saveMachine.machineName);
            if (machineInfo)
            {
                Machine machine = DoCreateMachine(machineInfo, saveMachine.bounds);
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
