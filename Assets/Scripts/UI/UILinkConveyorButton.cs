using UnityEngine;
using UnityEngine.UI;

public sealed class UILinkConveyorButton : MonoBehaviour
{
    public Sprite MachineOutputMaterial;
    public Sprite MachineInputMaterial;
    public Sprite ConveyorLinkMaterial;

    public Vector3Int position;
    public Vector3Int sourcePosition;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        UpdatePosition();
    }

    public void Initialize()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        ((RectTransform)transform).position = MainCamera.instance.WorldToScreenPoint(position.RoundToTileCenter());
    }

    void OnClick()
    {
        bool exists = ConveyorSystem.instance.conveyors.ContainsKey(position);
        Conveyor conveyor = ConveyorSystem.instance.GetOrCreateConveyor(sourcePosition, position, ConveyorCreateFlags.SelectConveyor | ConveyorCreateFlags.PanRelative);
        ConveyorSystem.instance.conveyors.TryGetValue(sourcePosition, out Conveyor sourceConveyor);
        if ((conveyor && conveyor.machine) || (sourceConveyor && sourceConveyor.machine))
        {
            Analytics.instance.NewUiEvent(UiEventId.ButtonLinkConveyorToMachine, 1);
        }
        else
        {
            Analytics.instance.NewUiEvent(UiEventId.ButtonLinkConveyor, 1);
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
