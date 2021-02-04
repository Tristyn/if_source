using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public sealed class UILinkConveyorButton : MonoBehaviour, IUpdate
{
    public Sprite MachineOutputMaterial;
    public Sprite MachineInputMaterial;
    public Sprite ConveyorLinkMaterial;

    public Vector3Int position;
    public Vector3Int sourcePosition;

    void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoUpdate()
    {
        UpdatePosition();
    }

    public void Initialize()
    {
        Entities.linkConveyorButtons.Add(this);
        UpdatePosition();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdatePosition()
    {
        ((RectTransform)transform).position = MainCamera.instance.WorldToScreenPoint(position.RoundToTileCenter());
    }

    void OnClick()
    {
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
        Entities.linkConveyorButtons.Remove(this);
        ObjectPooler.instance.Recycle(this);
    }
}
