using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class TileSelectionState
{
    public Bounds3Int bounds;
    public Conveyor conveyor;
    public Machine machine;

    public bool isSelected => conveyor || machine;
}

public sealed class TileSelectionManager : Singleton<TileSelectionManager>
{
    public TileSelectionState state = new TileSelectionState();
    public UIDemolishButton demolishButton;
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
        demolishButton.gameObject.SetActive(false);
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
        if (state.machine)
        {
            save = new Save
            {
                machinePosition = state.machine.bounds.min,
            };
        }
        else if (state.conveyor)
        {
            save = new Save
            {
                conveyorPosition = state.conveyor.save.position_local
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
        else if (state.conveyor)
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
        state = new TileSelectionState();
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
            state = new TileSelectionState
            {
                machine = selection.machine,
                bounds = selection.machine.bounds
            };
        }
        else
        {
            state = new TileSelectionState
            {
                conveyor = selection,
                bounds = selection.save.position_local.ToBounds()
            };
        }
        UpdateSelection();
    }

    public void SetSelection(Machine selection)
    {
        state = new TileSelectionState
        {
            machine = selection,
            bounds = selection.bounds
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

        demolishButton.demolishTile = state.bounds.min;
        demolishButton.gameObject.SetActive(state.isSelected);
        if (state.isSelected)
        {
            selectionHighlighter = ObjectPooler.instance.Get<SelectionHighlighter>();
            selectionHighlighter.Initialize(state.bounds);
            if (TouchInput.inputMode == InputMode.Touch)
            {
                foreach ((Vector3Int outerTile, Vector3Int innerTile) in state.bounds.EnumeratePerimeter())
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
    }

    public void TrySelectAnyInput(bool pan)
    {
        if (state.machine)
        {
            Conveyor[] conveyors = state.machine.conveyors;
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
        else if (state.conveyor)
        {
            Conveyor input = TryGetAnyInput(state.conveyor);
            if (input)
            {
                if (pan)
                {
                    Vector3 deltaPosition = input.save.position_local - state.conveyor.save.position_local;
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
