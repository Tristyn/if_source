using UnityEngine;
using UnityEngine.UI;

public sealed class UIDemolishButton : MonoBehaviour
{
    public Vector3Int demolishTile;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Machine machine = MachineSystem.instance.GetMachine(demolishTile);
        if (machine)
        {
            if (TileSelectionManager.instance.state.machine == machine)
            {
                TileSelectionManager.instance.TrySelectAnyInput(true);
            }
            machine.Demolish();
        }
        else if (ConveyorSystem.instance.conveyors.TryGetValue(demolishTile, out Conveyor conveyor))
        {
            if (TileSelectionManager.instance.state.conveyor == conveyor)
            {
                TileSelectionManager.instance.TrySelectAnyInput(true);
            }
            conveyor.Demolish();
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
