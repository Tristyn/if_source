using UnityEngine;

public class UIRotate : MonoBehaviour
{
    OverviewCameraController cameraController;

    void Awake()
    {
        cameraController = GetComponentInParent<OverviewCameraController>();
    }

    public void RotateLeft()
    {
        cameraController.Rotate(90);
    }

    public void RotateRight()
    {
        cameraController.Rotate(-90);
    }
}
