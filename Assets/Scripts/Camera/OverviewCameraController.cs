using System;
using UnityEngine;

[Serializable]
public struct ZoomIncrement
{
    public float height;
    public float pitch;
}

public class OverviewCameraController : Singleton<OverviewCameraController>
{
    CameraState targetCameraState;
    CameraState interpolatingCameraState;
    float accumulatedScroll;

    public float panSpeed = 5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target.")]
    public float positionLerpTime = 0.2f;
    float currentPositionLerpTime;

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;
    float currentRotationLerpTime;

    public float distanceWhenRotating;

    public float zoomHeightIncrement = 2f;
    public float zoomHeightMin = 2.5f;
    public float zoomHeightMax = 6.5f;
    public float pitch = 45;
    public float zoomInPitch = 30;

    public ZoomIncrement[] zoomIncrements;
    [SerializeField]
    public int zoomIncrement;

    void OnEnable()
    {
        targetCameraState.SetFromTransform(transform);
        interpolatingCameraState.SetFromTransform(transform);
        currentPositionLerpTime = positionLerpTime;
        currentRotationLerpTime = rotationLerpTime;
    }

    Vector3 GetKeyboardTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        return direction;
    }

    /// <summary>
    /// Pans relative to the cameras direction using speed
    /// </summary>
    public void PanLocal(Vector2 translation_local)
    {
        Vector3 translation_local3 = new Vector3(translation_local.x, 0, translation_local.y);
        Vector3 cameraDirection = interpolatingCameraState.eulerAngles;
        cameraDirection.y = 0; // Only pan on xz
        cameraDirection.Normalize();
        Vector3 translation_world = Quaternion.Euler(cameraDirection) * translation_local3 * panSpeed;
        targetCameraState.TranslateLocal(translation_world);
    }

    /// <summary>
    /// Translates the target position in world space
    /// </summary>
    public void MoveWorld(Vector3 translation_world)
    {
        targetCameraState.TranslateWorld(translation_world);
    }

    public void Rotate(float angle)
    {
        Vector3 yaw = new Vector3(0, targetCameraState.eulerAngles.y, 0);
        Vector3 pivot = targetCameraState.position + Quaternion.Euler(yaw) * new Vector3(0f, 0f, distanceWhenRotating);

        Vector3 eulerRotation = new Vector3(0, angle, 0);

        targetCameraState.position = Vector3Extensions.RotatePositionAroundPivot(targetCameraState.position, pivot, eulerRotation);
        targetCameraState.eulerAngles += eulerRotation;

        currentPositionLerpTime = positionLerpTime * 2.2f;
        currentRotationLerpTime = rotationLerpTime * 2;
    }

    public void SetZoomIncrement(int zoomIncrement)
    {
        this.zoomIncrement = Mathf.Clamp(zoomIncrement, 0, zoomIncrements.Length - 1);

        targetCameraState.position.y = zoomIncrements[this.zoomIncrement].height;
        targetCameraState.eulerAngles.x = zoomIncrements[this.zoomIncrement].pitch;
    }

    void Update()
    {
        // Exit Sample
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(90f);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(-90f);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetZoomIncrement(zoomIncrement + 1);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetZoomIncrement(zoomIncrement - 1);
        }
        int scrolldelta = Mathf.FloorToInt(Input.mouseScrollDelta.y + accumulatedScroll) - Mathf.FloorToInt(accumulatedScroll);
        accumulatedScroll += Input.mouseScrollDelta.y;
        if (scrolldelta != 0)
        {
            SetZoomIncrement(zoomIncrement - scrolldelta);
        }

        // Translation
        Vector3 translation = GetKeyboardTranslationDirection() * panSpeed * Time.deltaTime;
        // Speed up movement when shift key held
        if (Input.GetKey(KeyCode.LeftShift))
        {
            translation *= 2.2f;
        }

        targetCameraState.TranslateLocal(translation);

        currentPositionLerpTime = Mathf.Lerp(currentPositionLerpTime, positionLerpTime, Time.deltaTime);
        currentRotationLerpTime = Mathf.Lerp(currentRotationLerpTime, rotationLerpTime, Time.deltaTime);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / currentPositionLerpTime * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / currentRotationLerpTime * Time.deltaTime);
        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);

        interpolatingCameraState.UpdateTransform(transform);
    }
}