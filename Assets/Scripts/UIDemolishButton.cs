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
        bool audioPlayed = false;
        if(MachineSystem.instance.GetMachine(bounds.center.RoundToTile(), out Machine machine))
        {
            if (TileSelectionManager.instance.state.machine == machine)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            if (!audioPlayed)
            {
                audioPlayed = true;
                machine.PlayDemolishAudio();
            }
            machine.Delete();
        }
        else if(ConveyorSystem.instance.conveyors.TryGetValue(bounds.center.RoundToTile(), out Conveyor conveyor))
        {
            if (TileSelectionManager.instance.state.conveyor == conveyor)
            {
                TileSelectionManager.instance.SelectInput(true);
            }
            if (!audioPlayed)
            {
                audioPlayed = true;
                conveyor.PlayDemolishAudio();
            }
            conveyor.Recycle();
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
