public sealed class PuzzleGoals : Singleton<PuzzleGoals>
{
    public GoalInfo[] goals;

    public int goalIndex;
    public EfficiencyQuest efficiencyQuest;

    public void BeginCampaign()
    {
        goalIndex = 0;
        if (goalIndex < goals.Length)
        {
            BeginGoal(goals[goalIndex]);
        }
    }

    void BeginGoal(GoalInfo goalInfo)
    {
        if (efficiencyQuest != null)
        {
            efficiencyQuest.Delete();
        }

        for (int i = 0, len = goalInfo.machinesUnlockedAtStart.Length; i < len; ++i)
        {
            MachineUnlockSystem.instance.Unlock(goalInfo.machinesUnlockedAtStart[i]);
        }

        if (goalInfo.restrictLand)
        {
            foreach (LandParcel restrict in LandSystem.instance.landParcelSet)
            {
                if (restrict.flags == LandParcelFlags.Valid)
                {
                    restrict.flags = LandParcelFlags.Restricted;
                }
            }
        }

        if (goalInfo.createAddon)
        {
            Bounds3Int[] spacePlatformBounds = AddonGen.Addon(goalInfo.addon);
            SpacePlatform spacePlatform = new SpacePlatform();
            spacePlatform.save.bounds = spacePlatformBounds;
            spacePlatform.Initialize();
            OverviewCameraController.instance.MoveTo(spacePlatform.visual.floors[0].transform.position);
        }

        efficiencyQuest = new EfficiencyQuest
        {
            goalInfo = goalInfo
        };
        efficiencyQuest.Initialize();
    }

    public void EndGoal()
    {
        efficiencyQuest.Delete();
        efficiencyQuest = null;
    }

    public void DoFixedUpdate()
    {
        if (efficiencyQuest != null)
        {
            efficiencyQuest.Tick();
            if (efficiencyQuest.completed)
            {
                EndGoal();
                goalIndex++;
                if (goalIndex < goals.Length)
                {
                    BeginGoal(goals[goalIndex]);
                }
            }
        }
    }
}
