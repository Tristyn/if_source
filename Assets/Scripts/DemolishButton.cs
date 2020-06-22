using UnityEngine;
using UnityEngine.UI;

public class DemolishButton : MonoBehaviour
{
    public Vector2 screenOffset = new Vector2(0,-100);

    public OverviewCameraController cameraController;
    public Canvas canvas;
    public Bounds3Int bounds;

    RectTransform canvasRect;
    Button button;

    void Awake()
    {
        canvasRect = canvas.GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        UpdatePosition();
    }

    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        RectTransform rectTransform = (RectTransform)transform;
        rectTransform.position = (Vector2)cameraController.mainCamera.WorldToScreenPoint(bounds.center) + screenOffset * canvasRect.rect.size;
    }

    void OnClick()
    {
        if(MachineSystem.instance.GetMachine(bounds.center.RoundToTile(), out Machine machine))
        {
            if (SelectionManager.instance.selectionState.machine == machine)
            {
                SelectionManager.instance.SetSelection(null);
            }
            machine.Delete();
        }
        else if(ConveyorSystem.instance.conveyors.TryGetValue(bounds.center.RoundToTile(), out Conveyor conveyor))
        {
            if (SelectionManager.instance.selectionState.conveyor == conveyor)
            {
                SelectionManager.instance.SelectInput(true);
            }
            conveyor.Recycle();
        }
    }

    public void Recycle()
    {
        cameraController = null;
        canvas = null;
        canvasRect = null;
        ObjectPooler.instance.Recycle(this);
    }
}
