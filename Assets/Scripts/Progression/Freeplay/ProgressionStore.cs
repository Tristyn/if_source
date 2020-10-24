using System.Collections.Generic;
using UnityEngine;

public sealed class ProgressionStore : Singleton<ProgressionStore>
{
    public struct Save
    {
        public int[] completedProgressionIds;
    }

    HashSet<int> completedProgressionIds = new HashSet<int>();

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void GetSave(out Save save)
    {
        int[] completedProgressionIds = new int[this.completedProgressionIds.Count];
        save.completedProgressionIds = completedProgressionIds;
        int i = 0;
        foreach (var id in this.completedProgressionIds)
        {
            completedProgressionIds[i] = id;
            i++;
        }
    }

    public void SetSave(in Save save)
    {
        completedProgressionIds.Clear();
        if (save.completedProgressionIds != null)
        {
            completedProgressionIds.AddArray(save.completedProgressionIds);
        }
    }

    public bool GetProgressionComplete(ProgressionInfo progressionInfo)
    {
        return completedProgressionIds.Contains(progressionInfo.progressionId);
    }

    public void SetProgressionComplete(ProgressionInfo progressionInfo, bool complete)
    {
        if (complete)
        {
            completedProgressionIds.Add(progressionInfo.progressionId);
        }
        else
        {
            completedProgressionIds.Remove(progressionInfo.progressionId);
        }
    }
}
