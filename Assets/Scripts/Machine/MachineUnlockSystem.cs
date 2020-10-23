using System;
using System.Collections.Generic;
using UnityEngine;

public class MachineUnlockSystem : Singleton<MachineUnlockSystem>
{
    [NonSerialized]
    public HashSet<MachineInfo> unlocked = new HashSet<MachineInfo>();

    public struct Save
    {
        public string[] unlocked;
    }

    public void GetSave(out Save save)
    {
        string[] unlockedSave = new string[unlocked.Count];
        save.unlocked = unlockedSave;
        int i = 0;
        foreach (MachineInfo unlockedMachineInfo in unlocked)
        {
            unlockedSave[i] = unlockedMachineInfo.machineName;
            ++i;
        }
    }

    public void SetSave(in Save save)
    {
        unlocked.Clear();
        string[] unlockedSave = save.unlocked;
        if (unlockedSave != null)
        {
            for (int i = 0, len = unlockedSave.Length; i < len; ++i)
            {
                MachineInfo machineInfo = ScriptableObjects.instance.GetMachineInfo(unlockedSave[i]);
                if (machineInfo)
                {
                    unlocked.Add(machineInfo);
                }
                else
                {
                    Debug.LogWarning($"Failed to find machine info {unlockedSave[i]} while loading machine unlock.");
                }
            }
        }
    }

    public void Unlock(MachineInfo machineInfo)
    {
        if (!unlocked.Contains(machineInfo))
        {
            unlocked.Add(machineInfo);
            Events.machineUnlocked?.Invoke(machineInfo);
        }
    }

    public void UnlockAll()
    {
        MachineInfo[] machines = ScriptableObjects.instance.machines;
        for (int i = 0, len = machines.Length; i < len; ++i)
        {
            Unlock(machines[i]);
        }
    }
}
