using UnityEngine;
using UnityEngine.UI;
using UnityTemplateProjects;

public class UILinkConveyorButton : MonoBehaviour
{
    public Sprite MachineOutputMaterial;
    public Sprite MachineInputMaterial;
    public Sprite ConveyorLinkMaterial;

    public OverviewCameraController cameraController;
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
        RectTransform rectTransform = (RectTransform)transform;
        rectTransform.position = cameraController.mainCamera.WorldToScreenPoint(position);
    }

    void OnClick()
    {
        Conveyor conveyor = Conveyor.CreateConveyor(sourcePosition, position);
        if (conveyor)
        {
            Vector3 deltaPosition = (position - sourcePosition);
            TileSelectionManager.instance.PanTo(conveyor);
            TileSelectionManager.instance.SetSelection(conveyor);
            // Calculate deltaposition first because after SetSelection is called this button is likely recycled.
        }
    }

    public void Recycle()
    {
        cameraController = null;
        ObjectPooler.instance.Recycle(this);
    }
}
