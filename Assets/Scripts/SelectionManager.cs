using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public enum ButtonState
{
    None, Select, Demolish, BuildConveyor
}

public class SelectionState
{
    public Bounds3Int? bounds;
    public Conveyor conveyor;
    public Machine machine;
}

public class SelectionManager : Singleton<SelectionManager>
{
    public ButtonState buttonState;
    public SelectionState selectionState = new SelectionState();
    public SelectionHighlighter selectionHighlighter;
    public DemolishButton demolishButton;
    public Canvas canvas;
    public OverviewCameraController cameraController;

    private List<ConveyorButton> conveyorButtons = new List<ConveyorButton>(16);

    protected override void Awake()
    {
        base.Awake();
        demolishButton.gameObject.SetActive(false);
    }

    public void SetSelection(Conveyor selection)
    {
        selectionState = new SelectionState
        {
            conveyor = selection,
        };
        if (selection)
        {
            selectionState.bounds = selection.position.ToBounds();
        }

        UpdateSelection();
    }

    public void SelectInput(bool pan)
    {
        if (selectionState.conveyor)
        {
            Conveyor[] inputs = selectionState.conveyor.inputs;
            for (int i = 0, len = inputs.Length; i < len; i++)
            {
                if (inputs[i])
                {
                    if (pan)
                    {
                        PanTo(inputs[i]);
                    }
                    SetSelection(inputs[i]);
                    return;
                }
            }
        }
        SetSelection(null);
    }

    public void PanTo(Conveyor selection)
    {
        if (selection && selectionState.conveyor)
        {
            Vector3 deltaPosition = selection.position - selectionState.conveyor.position;
            cameraController.MoveWorld(deltaPosition);
        }
    }

    void UpdateSelection()
    {
        for (int i = 0, len = conveyorButtons.Count; i < len; i++)
        {
            conveyorButtons[i].Recycle();
        }
        conveyorButtons.Clear();

        demolishButton.bounds = selectionState.bounds ?? default;
        demolishButton.gameObject.SetActive(selectionState.bounds.HasValue);
        selectionHighlighter.Highlight(selectionState.conveyor);

        if (selectionState.bounds.HasValue)
        {
            foreach ((Vector3Int outerTile, Vector3Int innerTile) in selectionState.bounds.Value.EnumeratePerimeter())
            {
                ConveyorButton conveyorButton = ObjectPooler.instance.Get<ConveyorButton>();
                conveyorButton.position = outerTile;
                conveyorButton.sourcePosition = innerTile;
                conveyorButton.cameraController = cameraController;
                conveyorButton.transform.SetParent(canvas.transform, false);
                conveyorButton.Initialize();
                conveyorButtons.Add(conveyorButton);
            }
        }
    }
}
