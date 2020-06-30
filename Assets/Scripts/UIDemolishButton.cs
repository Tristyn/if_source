using UnityEngine;
using UnityEngine.UI;

public class UIDemolishButton : MonoBehaviour
{
    public Bounds3Int bounds;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if(MachineSystem.instance.GetMachine(bounds.center.RoundToTile(), out Machine machine))
        {
            if (TileSelectionManager.instance.state.machine == machine)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            machine.Delete();
        }
        else if(ConveyorSystem.instance.conveyors.TryGetValue(bounds.center.RoundToTile(), out Conveyor conveyor))
        {
            if (TileSelectionManager.instance.state.conveyor == conveyor)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            conveyor.Recycle();
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
