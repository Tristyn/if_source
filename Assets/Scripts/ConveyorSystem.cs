using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Flags]
public enum ConveyorCreateFlags
{
    None = 0,
    SilenceAudio = 1,
    SelectConveyor = 2,
    PanRelative = 4
}

public sealed class ConveyorSystem : Singleton<ConveyorSystem>
{
    public Dictionary<Vector3Int, Conveyor> conveyors = new Dictionary<Vector3Int, Conveyor>();
    public AudioClip createConveyorClip;
    public AudioClip demolishConveyorClip;

    public struct Save
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Conveyor.Save[] conveyors;
    }
    public Save save;

    protected override void Awake()
    {
        base.Awake();
        Init.PreSave += PreSave;
        Init.PostSave += PostSave;
        Init.PreLoad += PreLoad;
        Init.PostLoad += PostLoad;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Init.PreSave -= PreSave;
        Init.PostSave -= PostSave;
        Init.PreLoad -= PreLoad;
        Init.PostLoad -= PostLoad;
    }

    void PreSave()
    {
        Save save = new Save
        {
            conveyors = new Conveyor.Save[conveyors.Count]
        };
        this.save = save;
        int i = 0;
        foreach (Conveyor conveyor in conveyors.Values)
        {
            conveyor.GetSave(out save.conveyors[i]);
            ++i;
        }
    }

    void PostSave()
    {
        save = default;
    }

    void PreLoad()
    {
        Conveyor[] conveyorClones = new Conveyor[conveyors.Count];
        conveyors.Values.CopyTo(conveyorClones, 0);
        for (int i = 0, len = conveyorClones.Length; i < len; ++i)
        {
            conveyorClones[i].Recycle();
        }
    }

    void PostLoad()
    {
        Conveyor.Save[] saveConveyors = save.conveyors;
        for (int i = 0, len = saveConveyors.Length; i < len; i++)
        {
            DoCreateConveyor(saveConveyors[i].position_local);
        }
        
        for (int i = 0, len = saveConveyors.Length; i < len; i++)
        {
            ref Conveyor.Save saveConveyor = ref saveConveyors[i];
            Conveyor conveyor = conveyors[saveConveyor.position_local];
            conveyor.SetSave(in saveConveyor);
        }
        save = default;
    }

    public bool CanCreate(Vector3Int position)
    {
        return !MachineSystem.instance.GetMachine(position);
    }

    public bool CanLink(Vector3Int from, Vector3Int to)
    {
        Machine toMachine = MachineSystem.instance.GetMachine(to);
        if (toMachine && !toMachine.canInput)
        {
            return false;
        }
        Machine fromMachine = MachineSystem.instance.GetMachine(from);
        if (fromMachine && !fromMachine.canOutput)
        {
            return false;
        }

        // Cannot link from machine to machine, or within the same machine
        if (toMachine && fromMachine)
        {
            return false;
        }
        return true;
    }

    public Conveyor CreateConveyor(Vector3Int position, ConveyorCreateFlags flags)
    {
        if (!CanCreate(position))
        {
            return null;
        }
        if (conveyors.ContainsKey(position))
        {
            return null;
        }

        Conveyor conveyor = DoCreateConveyor(position);
        ProcessFlags(flags, numConveyorsCreated: 1, wereConveyorsLinked: false, conveyor);
        Assert.IsFalse((flags & ConveyorCreateFlags.PanRelative) != 0, "Pan relative does nothing here");
        return conveyor;
    }

    public Conveyor GetOrCreateConveyor(Vector3Int position, ConveyorCreateFlags flags)
    {
        if (!CanCreate(position))
        {
            return null;
        }
        if (conveyors.TryGetValue(position, out Conveyor conveyor))
        {
            return conveyor;
        }

        conveyor = DoCreateConveyor(position);

        Assert.IsFalse((flags & ConveyorCreateFlags.PanRelative) != 0, "Pan relative does nothing here");
        ProcessFlags(flags, numConveyorsCreated: 1, wereConveyorsLinked: false, conveyor);

        return conveyor;
    }

    public Conveyor GetOrCreateConveyor(Vector3Int fromPosition, Vector3Int toPosition, ConveyorCreateFlags flags)
    {
        if (!CanLink(fromPosition, toPosition))
        {
            return null;
        }

        if (!fromPosition.IsNeighbor(toPosition))
        {
            Debug.LogWarning("Can't link conveyors that aren't neighbors.");
            return null;
        }

        int numConveyorsCreated = 0;
        if (!conveyors.TryGetValue(toPosition, out Conveyor to))
        {
            to = DoCreateConveyor(toPosition);
            ++numConveyorsCreated;
        }
        if (!conveyors.TryGetValue(fromPosition, out Conveyor from))
        {
            from = DoCreateConveyor(fromPosition);
            ++numConveyorsCreated;
        }

        bool wereConveyorsLinked = false;
        if (!from.IsLinked(to))
        {
            from.Link(to);
            wereConveyorsLinked = true;
        }

        ProcessFlags(flags, numConveyorsCreated, wereConveyorsLinked, from, to);
        return to;
    }

    private Conveyor DoCreateConveyor(Vector3Int position)
    {
        Conveyor conveyor = ObjectPooler.instance.Get<Conveyor>();
        conveyor.transform.localPosition = position.RoundToTileCenter();
        conveyor.Initialize();
        return conveyor;
    }

    void ProcessFlags(ConveyorCreateFlags flags, int numConveyorsCreated, bool wereConveyorsLinked, Conveyor from, Conveyor to)
    {
        // Do PanRelative before SelectConveyor
        if ((flags & ConveyorCreateFlags.PanRelative) != 0 && from && to)
        {
            Vector3 deltaPosition = to.save.position_local - from.save.position_local;
            OverviewCameraController.instance.MoveWorld(deltaPosition);
        }
        ProcessFlags(flags, numConveyorsCreated, wereConveyorsLinked, to);
    }

    void ProcessFlags(ConveyorCreateFlags flags, int numConveyorsCreated, bool wereConveyorsLinked, Conveyor conveyor)
    {
        bool playAudio = numConveyorsCreated > 0 || wereConveyorsLinked;
        if (playAudio && (flags & ConveyorCreateFlags.SilenceAudio) == 0)
        {
            PlayCreateAudio();
        }
        if ((flags & ConveyorCreateFlags.SelectConveyor) != 0)
        {
            TileSelectionManager.instance.SetSelection(conveyor);
        }
    }

    void PlayCreateAudio()
    {
        AudioSystem.instance.PlayOneShot(createConveyorClip, AudioCategory.Effect);
    }

    public void PlayDemolishAudio()
    {
        AudioSystem.instance.PlayOneShot(demolishConveyorClip, AudioCategory.Effect);
    }
}
