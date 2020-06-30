using UnityEngine;
using UnityTemplateProjects;

public class UIRotate : MonoBehaviour
{
    OverviewCameraController cameraController;

    void Awake()
    {
        cameraController = GetComponentInParent<OverviewCameraController>();

        Transform touchControls = transform.Find("Touch Controls");
        if (touchControls)
        {
            touchControls.gameObject.SetActive(Input.touchSupported);
        }
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
