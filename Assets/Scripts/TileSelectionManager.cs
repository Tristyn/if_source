using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class TileSelectionState
{
    public Bounds3Int bounds;
    public Conveyor conveyor;
    public Machine machine;

    public bool isSelected => conveyor || machine;
}

public class TileSelectionManager : Singleton<TileSelectionManager>
{
    public TileSelectionState state = new TileSelectionState();
    public UIDemolishButton demolishButton;
    public Canvas canvas;

    private List<UILinkConveyorButton> conveyorButtons = new List<UILinkConveyorButton>(16);
    private SelectionHighlighter selectionHighlighter;

    protected override void Awake()
    {
        base.Awake();
        demolishButton.gameObject.SetActive(false);
    }

    public void SetSelection()
    {
        state = new TileSelectionState();
        UpdateSelection();
    }

    public bool SetSelection(Vector3Int tile)
    {
        if(MachineSystem.instance.GetMachine(tile, out Machine machine))
        {
            SetSelection(machine);
            return true;
        }
        else if(ConveyorSystem.instance.conveyors.TryGetValue(tile, out Conveyor conveyor))
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
                bounds = selection.position.ToBounds()
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
        for (int i = 0, len = conveyorButtons.Count; i < len; i++)
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

    public void SelectInput(bool pan)
    {
        if (state.machine)
        {
            Conveyor[] conveyors = state.machine.conveyors;
            for (int i = 0, len = conveyors.Length; i < len; i++)
            {
                Conveyor conveyor = conveyors[i];
                Conveyor[] inputs = conveyor.inputs;
                for (int j = 1, jLen = inputs.Length; j < jLen; j++)
                {
                    Conveyor input = inputs[j];
                    if (input)
                    {
                        if (pan)
                        {
                            Vector3 deltaPosition = input.position - conveyor.position;
                            OverviewCameraController.instance.MoveWorld(deltaPosition);
                        }
                        SetSelection(input);
                        return;
                    }
                }
            }
        }
        else if (state.conveyor)
        {
            Conveyor[] inputs = state.conveyor.inputs;
            for (int i = 0, len = inputs.Length; i < len; i++)
            {
                Conveyor input = inputs[i];
                if (input)
                {
                    if (pan)
                    {
                        Vector3 deltaPosition = input.position - state.conveyor.position;
                        OverviewCameraController.instance.MoveWorld(deltaPosition);
                    }
                    SetSelection(input);
                    return;
                }
            }
        }
        SetSelection();
    }
}
