using UnityEngine;
using UnityEngine.UI;

public class UIDemolishButton : MonoBehaviour
{
    public Vector3Int demolishTile;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (MachineSystem.instance.GetMachine(demolishTile, out Machine machine))
        {
            if (TileSelectionManager.instance.state.machine == machine)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            machine.PlayDemolishAudio();
            machine.Delete();
        }
        else if (ConveyorSystem.instance.conveyors.TryGetValue(demolishTile, out Conveyor conveyor))
        {
            if (TileSelectionManager.instance.state.conveyor == conveyor)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            conveyor.PlayDemolishAudio();
            conveyor.Recycle();
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
