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
    RectTransform rectTransform;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        rectTransform = (RectTransform)transform; // Casting this early probably has no performance benefit
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
        rectTransform.position = OverviewCameraController.instance.mainCamera.WorldToScreenPoint(position.RoundToTileCenter());
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
