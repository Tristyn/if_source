using UnityEngine;

public sealed class UIRotate : MonoBehaviour
{
    public void RotateLeft()
    {
        OverviewCameraController.instance.Rotate(90);
        Analytics.instance.NewUiEvent(UiEventId.ButtonRotateLeft, 1);
    }

    public void RotateRight()
    {
        OverviewCameraController.instance.Rotate(-90);
        Analytics.instance.NewUiEvent(UiEventId.ButtonRotateRight, 1);
    }
}
