using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIDemolishButton : MonoBehaviour
{
    Vector3Int? demolishTile;
    UIBehaviour[] uiBehaviours;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        uiBehaviours = GetComponentsInChildren<UIBehaviour>();
        uiBehaviours.SetEnabled(demolishTile.HasValue);
        Events.TileSelectionChanged += TileSelectionChanged;
    }

    void OnDestroy()
    {
        Events.TileSelectionChanged -= TileSelectionChanged;
    }

    void TileSelectionChanged(SelectionState selectionState)
    {
        demolishTile = selectionState.isSelected ? selectionState.bounds.min : (Vector3Int?)null;
        uiBehaviours.SetEnabled(selectionState.isSelected);
    }

    void OnClick()
    {
        if (demolishTile.HasValue)
        {
            Machine machine = MachineSystem.instance.GetMachine(demolishTile.Value);
            if (machine)
            {
                if (TileSelectionManager.instance.selectionState.machine == machine)
                {
                    TileSelectionManager.instance.TrySelectAnyInput(true);
                }
                machine.Demolish();
                Analytics.instance.NewUiEvent(UiEventId.ButtonDemolish, 1);
            }
            else if (ConveyorSystem.instance.conveyors.TryGetValue(demolishTile.Value, out Conveyor conveyor))
            {
                if (TileSelectionManager.instance.selectionState.conveyor == conveyor)
                {
                    TileSelectionManager.instance.TrySelectAnyInput(true);
                }
                conveyor.Demolish();
                Analytics.instance.NewUiEvent(UiEventId.ButtonDemolish, 1);
            }
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
