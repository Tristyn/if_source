using UnityEngine;

[CreateAssetMenu(fileName = "NewGoal", menuName = "Goal", order = 30)]
public sealed class GoalInfo : ScriptableObject
{
    public bool restrictLand;
    public bool createAddon;
    public AddonParameters addon;
    public MachineInfo[] machinesUnlockedAtStart;
    public MachineInfo goalMachineInfo;
    public int numMachines;
    public float goalEfficiency = 100;
}
