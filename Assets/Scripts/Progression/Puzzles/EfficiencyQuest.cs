using System;
using UnityEngine;

public sealed class EfficiencyQuest
{
    public GoalInfo goalInfo;

    public Machine[] targetMachines = Array.Empty<Machine>();
    public float progress;
    public bool completed;

    public void Initialize()
    {
        Events.MachineCreated += OnMachineCreated;
    }

    public void Delete()
    {
        Events.MachineCreated -= OnMachineCreated;
    }

    void OnMachineCreated(Machine machine)
    {
        if (goalInfo.goalMachineInfo == machine.machineInfo)
        {
            targetMachines = targetMachines.Append(machine);
        }
    }

    public void Tick()
    {
        progress = CalculateProgress();
        completed = Mathf.Approximately(progress, goalInfo.goalEfficiency);
    }

    float CalculateProgress()
    {
        float progress = 0f;
        for (int i = 0, len = targetMachines.Length; i < len; ++i)
        {
            progress += targetMachines[i].machineEfficiency.efficiency;
        }
        return progress / (goalInfo.goalEfficiency * goalInfo.numMachines);
    }
}
