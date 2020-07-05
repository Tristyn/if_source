using UnityEngine;

public class UIRotate : MonoBehaviour
{
    public void RotateLeft()
    {
        OverviewCameraController.instance.Rotate(90);
    }

    public void RotateRight()
    {
        OverviewCameraController.instance.Rotate(-90);
    }
}
