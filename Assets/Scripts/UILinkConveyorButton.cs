using UnityEngine;
using UnityEngine.UI;

public class UILinkConveyorButton : MonoBehaviour
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
        Conveyor conveyor = Conveyor.CreateConveyor(sourcePosition, position);
        if (conveyor)
        {
            Vector3 deltaposition = position - sourcePosition;
            OverviewCameraController.instance.MoveWorld(deltaposition);
            TileSelectionManager.instance.SetSelection(conveyor);
        }
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
