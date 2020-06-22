using System;
using System.Collections.Generic;
using UnityEngine;

public class MachineSystem : Singleton<MachineSystem>
{
    [NonSerialized]
    public List<Machine> machines = new List<Machine>();

    public bool GetMachine(Vector3Int position, out Machine machine)
    {
        for (int i = 0, len = machines.Count; i < len; i++)
        {
            if (machines[i].bounds.Contains(position))
            {
                machine = machines[i];
                return true;
            }
        }
        machine = null;
        return false;
    }
}
