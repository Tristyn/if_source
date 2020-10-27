using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum SelectionMode
{
    None = 0,
    Conveyor = 1,
    Machine = 2
}

public sealed class SelectionState
{
    public SelectionMode selectionMode;
    public MachineInfo machineInfo;
    public Machine machine;
    public Conveyor conveyor;
    public Bounds3Int bounds;

    public bool isSelected => conveyor || machine;
}

public sealed class TileSelectionManager : Singleton<TileSelectionManager>
{
    public SelectionState selectionState = new SelectionState();
    public Canvas canvas;

    private List<UILinkConveyorButton> conveyorButtons = new List<UILinkConveyorButton>(16);
    private SelectionHighlighter selectionHighlighter;

    public Save save;

    public struct Save
    {
        public Vector3Int? conveyorPosition;
        public Vector3Int? machinePosition;
    }

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.PreSave += PreSave;
        SaveLoad.PreLoad += PreLoad;
        SaveLoad.LoadComplete += LoadComplete;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveLoad.PreSave -= PreSave;
        SaveLoad.PreLoad -= PreLoad;
        SaveLoad.LoadComplete -= LoadComplete;
    }

    void PreSave()
    {
        if (selectionState.machine)
        {
            save = new Save
            {
                machinePosition = selectionState.machine.bounds.min,
            };
        }
        else if (selectionState.conveyor)
        {
            save = new Save
            {
                conveyorPosition = selectionState.conveyor.save.position_local
            };
        }
        else
        {
            save = new Save();
        }
    }

    void PreLoad()
    {
        SetSelection();
    }

    void LoadComplete()
    {
        if (save.machinePosition.HasValue)
        {
            Machine machine = MachineSystem.instance.GetMachine(save.machinePosition.Value);
            Assert.IsNotNull(machine);
            SetSelection(machine);
        }
        else if (selectionState.conveyor)
        {
            ConveyorSystem.instance.conveyors.TryGetValue(save.machinePosition.Value, out Conveyor conveyor);
            Assert.IsNotNull(conveyor);
            SetSelection(conveyor);
        }
        else
        {
            SetSelection();
        }
    }

    public void SetSelection()
    {
        selectionState = new SelectionState();
        UpdateSelection();
    }

    public bool SetSelection(Vector3Int tile)
    {
        Machine machine = MachineSystem.instance.GetMachine(tile);
        if (machine)
        {
            SetSelection(machine);
            return true;
        }
        else if (ConveyorSystem.instance.conveyors.TryGetValue(tile, out Conveyor conveyor))
        {
            SetSelection(conveyor);
            return true;
        }
        return false;
    }

    public void SetSelection(Conveyor selection)
    {
        if (selection.machine)
        {
            selectionState = new SelectionState
            {
                selectionMode = SelectionMode.Machine,
                machine = selection.machine,
                machineInfo = selection.machine.machineInfo,
                conveyor = selection,
                bounds = selection.machine.bounds
            };
        }
        else
        {
            selectionState = new SelectionState
            {
                selectionMode = SelectionMode.Conveyor,
                conveyor = selection,
                bounds = selection.save.position_local.ToBounds()
            };
        }
        UpdateSelection();
    }

    public void SetSelection(Machine selection)
    {
        selectionState = new SelectionState
        {
            selectionMode = selection ? SelectionMode.Machine : SelectionMode.None,
            machineInfo = selection ? selection.machineInfo : null,
            machine = selection,
            bounds = selection ? selection.bounds : default
        };
        UpdateSelection();
    }

    void UpdateSelection()
    {
        for (int i = 0, len = conveyorButtons.Count; i < len; ++i)
        {
            conveyorButtons[i].Recycle();
        }
        conveyorButtons.Clear();
        if (selectionHighlighter)
        {
            selectionHighlighter.Recycle();
            selectionHighlighter = null;
        }

        if (selectionState.isSelected)
        {
            selectionHighlighter = ObjectPooler.instance.Get<SelectionHighlighter>();
            selectionHighlighter.Initialize(selectionState.bounds);
            if (TouchInput.inputMode == InputMode.Touch)
            {
                foreach ((Vector3Int outerTile, Vector3Int innerTile) in selectionState.bounds.EnumeratePerimeter())
                {
                    if (ConveyorSystem.instance.CanLink(innerTile, outerTile))
                    {
                        UILinkConveyorButton conveyorButton = ObjectPooler.instance.Get<UILinkConveyorButton>();
                        conveyorButton.position = outerTile;
                        conveyorButton.sourcePosition = innerTile;
                        conveyorButton.transform.SetParent(canvas.transform, false);
                        conveyorButton.Initialize();
                        conveyorButtons.Add(conveyorButton);
                    }
                }
            }
        }
        Events.TileSelectionChanged?.Invoke(selectionState);
    }

    public void TrySelectAnyInput(bool pan)
    {
        if (selectionState.machine)
        {
            Conveyor[] conveyors = selectionState.machine.conveyors;
            for (int i = 0, len = conveyors.Length; i < len; ++i)
            {
                Conveyor conveyor = conveyors[i];
                Conveyor input = TryGetAnyInput(conveyor);
                if (input)
                {
                    if (pan)
                    {
                        Vector3 deltaPosition = input.save.position_local - conveyor.save.position_local;
                        OverviewCameraController.instance.MoveWorld(deltaPosition);
                    }
                    SetSelection(input);
                    return;
                }
            }
        }
        else if (selectionState.conveyor)
        {
            Conveyor input = TryGetAnyInput(selectionState.conveyor);
            if (input)
            {
                if (pan)
                {
                    Vector3 deltaPosition = input.save.position_local - selectionState.conveyor.save.position_local;
                    OverviewCameraController.instance.MoveWorld(deltaPosition);
                }
                SetSelection(input);
                return;
            }
        }
        SetSelection();
    }

    private Conveyor TryGetAnyInput(Conveyor conveyor)
    {
        Directions[] directions = EnumUtil<Directions>.values;
        int directionsLen = directions.Length;
        for (int j = 0; j < directionsLen; ++j)
        {
            Conveyor input = conveyor.TryGetInput(directions[j]);
            if (input)
            {
                return input;
            }
        }
        return null;
    }
}
