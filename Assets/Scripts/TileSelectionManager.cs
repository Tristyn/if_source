using System.Collections.Generic;
using UnityEngine;

public class TileSelectionState
{
    public Bounds3Int? bounds;
    public Conveyor conveyor;
    public Machine machine;
}

public class TileSelectionManager : Singleton<TileSelectionManager>
{
    public TileSelectionState state = new TileSelectionState();
    public SelectionHighlighter selectionHighlighter;
    public UIDemolishButton demolishButton;
    public Canvas canvas;
    public OverviewCameraController cameraController;

    private List<UILinkConveyorButton> conveyorButtons = new List<UILinkConveyorButton>(16);

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

    public void SetSelection(Conveyor selection)
    {
        state = new TileSelectionState
        {
            conveyor = selection,
            bounds = selection.position.ToBounds()
        };
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

        demolishButton.bounds = state.bounds ?? default;
        demolishButton.gameObject.SetActive(state.bounds.HasValue);
        selectionHighlighter.Highlight(state.conveyor);

        if (state.bounds.HasValue)
        {
            foreach ((Vector3Int outerTile, Vector3Int innerTile) in state.bounds.Value.EnumeratePerimeter())
            {
                UILinkConveyorButton conveyorButton = ObjectPooler.instance.Get<UILinkConveyorButton>();
                conveyorButton.position = outerTile;
                conveyorButton.sourcePosition = innerTile;
                conveyorButton.cameraController = cameraController;
                conveyorButton.transform.SetParent(canvas.transform, false);
                conveyorButton.Initialize();
                conveyorButtons.Add(conveyorButton);
            }
        }
    }

    public void SelectInput(bool pan)
    {
        if (state.conveyor)
        {
            Conveyor[] inputs = state.conveyor.inputs;
            for (int i = 0, len = inputs.Length; i < len; i++)
            {
                Conveyor conveyor = inputs[i];
                if (conveyor)
                {
                    if (pan)
                    {
                        PanTo(conveyor);
                    }
                    SetSelection(conveyor);
                    return;
                }
            }
        }
        else if (state.machine)
        {
            Conveyor[] conveyors = state.machine.conveyors;
            for (int i = 0, len = conveyors.Length; i < len; i++)
            {
                Conveyor[] inputs = conveyors[i].inputs;
                for (int j = 1, jLen = inputs.Length; j < jLen; j++)
                {
                    Conveyor input = inputs[j];
                    if (input)
                    {
                        if (pan)
                        {
                            PanTo(input);
                        }
                        SetSelection(input);
                        return;
                    }
                }
            }
        }
        SetSelection();
    }

    public void PanTo(Conveyor selection)
    {
        if (selection && state.conveyor)
        {
            Vector3 deltaPosition = selection.position - state.conveyor.position;
            cameraController.MoveWorld(deltaPosition);
        }
    }
}
