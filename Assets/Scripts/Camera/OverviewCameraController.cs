using System;
using UnityEngine;

[Serializable]
public struct ZoomIncrement
{
    public float height;
    public float pitch;
    public float viewDiameter;
}

public sealed class OverviewCameraController : Singleton<OverviewCameraController>
{
    float accumulatedScroll;

    public float panSpeed = 5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target.")]
    public float positionLerpTime = 0.2f;
    float currentPositionLerpTime;

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;
    float currentRotationLerpTime;

    [Tooltip("Time it takes to interpolate camera field of view 99% of the way to the target."), Range(0.001f, 1f)]
    public float viewDiameterLerpTime = 0.01f;
    float currentViewDiameterLerpTime;

    public float distanceWhenRotating;

    public float zoomHeightIncrement = 2f;
    public float zoomHeightMin = 2.5f;
    public float zoomHeightMax = 6.5f;
    public float pitch = 45;
    public float zoomInPitch = 30;

    public ZoomIncrement[] zoomIncrements;

    public Save save;

    Camera mainCamera;


    [Serializable]
    public struct Save
    {
        public CameraState targetCameraState;
        public CameraState interpolatingCameraState;
        public int zoomIncrement;
        public bool enabled;
    }

    protected override void Awake()
    {
        base.Awake();
        Init.Bind += () =>
        {
            mainCamera = MainCamera.instance;
        };
        SaveLoad.PostLoad += PostLoad;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SaveLoad.PostLoad -= PostLoad;
    }

    void PostLoad()
    {
        SetEnabled(save.enabled);
    }

    Vector3 GetKeyboardTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (!TouchInput.IsKeyboardConsumedByUI())
        {
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
        }
        return direction;
    }

    /// <summary>
    /// Pans relative to the cameras direction using speed
    /// </summary>
    public void PanLocal(Vector2 translation_local)
    {
        Vector3 translation_local3 = new Vector3(translation_local.x, 0, translation_local.y);
        Vector3 cameraDirection = save.interpolatingCameraState.eulerAngles;
        cameraDirection.y = 0; // Only pan on xz
        cameraDirection.Normalize();
        Vector3 translation_world = Quaternion.Euler(cameraDirection) * translation_local3 * panSpeed;
        save.targetCameraState.TranslateLocal(translation_world);
    }

    /// <summary>
    /// Translates the target position in world space
    /// </summary>
    public void MoveWorld(Vector3 translation_world)
    {
        save.targetCameraState.TranslateWorld(translation_world);
    }

    /// <summary>
    /// Translates the target position in world space
    /// </summary>
    public void MoveTo(Vector3 position_world)
    {
        position_world.y = save.targetCameraState.position.y;
        save.targetCameraState.position = position_world;
    }

    public void SetRotation(float angle)
    {
        save.targetCameraState.eulerAngles.y = angle;
    }

    public void Rotate(float angle)
    {
        Vector3 yaw = new Vector3(0, save.targetCameraState.eulerAngles.y, 0);
        Vector3 pivot = save.targetCameraState.position + Quaternion.Euler(yaw) * new Vector3(0f, 0f, distanceWhenRotating);

        Vector3 eulerRotation = new Vector3(0, angle, 0);

        save.targetCameraState.position = Vector3Extensions.RotatePositionAroundPivot(save.targetCameraState.position, pivot, eulerRotation);
        save.targetCameraState.eulerAngles += eulerRotation;

        currentPositionLerpTime = positionLerpTime * 2.2f;
        currentRotationLerpTime = rotationLerpTime * 2;
    }

    public void SetZoomIncrement(int zoomIncrement)
    {
        save.zoomIncrement = Mathf.Clamp(zoomIncrement, 0, zoomIncrements.Length - 1);
        ref ZoomIncrement zoom = ref zoomIncrements[this.save.zoomIncrement];

        Ray cameraRay = save.targetCameraState.CameraCenterToRay();

        CameraState newCameraState = save.targetCameraState;
        newCameraState.position.y = zoom.height;
        newCameraState.eulerAngles.x = zoom.pitch;
        newCameraState.viewDiameter = zoom.viewDiameter;


        // Keep the reference point (the ground at camera center) the same by adjusting the target position
        Plane groundPlane = new Plane(new Vector3(0, 1, 0), 0);
        if (groundPlane.Raycast(cameraRay, out float enter) || enter != 0)
        {
            Vector3 groundIntersection = cameraRay.GetPoint(enter);
            Plane cameraAltitudePlane = new Plane(new Vector3(0, -1, 0), newCameraState.position.y);
            Ray fromGroundRay = newCameraState.CameraCenterToRay();
            fromGroundRay.origin = groundIntersection;

            if(cameraAltitudePlane.Raycast(fromGroundRay, out enter) || enter != 0)
            {
                Vector3 cameraAltitudeIntersection = fromGroundRay.GetPoint(enter);
                newCameraState.position = cameraAltitudeIntersection;
            }
        }

        save.targetCameraState = newCameraState;
    }

    public void SnapToTargetState()
    {
        save.interpolatingCameraState = save.targetCameraState;
        save.interpolatingCameraState.UpdateTransform(transform);
        mainCamera.fieldOfView = GetFov(in save.interpolatingCameraState, mainCamera.aspect);
    }

    public void SetInitialState()
    {
        SetZoomIncrement(int.MaxValue);
        SetRotation(90f);
        MoveTo(new Vector3(-12.75f, 0, 0));
        SnapToTargetState();
    }

    float GetFov(in CameraState cameraState, float aspect)
    {
        Plane groundPlane = new Plane(new Vector3(0, 1, 0), 0.8f);
        Ray cameraRay = cameraState.CameraCenterToRay();
        if (!groundPlane.Raycast(cameraRay, out float distanceToGround))
        {
            distanceToGround = 15f;
        }

        // Calculate based on the smaller of frustum width and height
        float viewDiameter = cameraState.viewDiameter;
        if (aspect < 1)
        {
            // Landscape screen, calculate vertical frustum
            viewDiameter /= aspect;
        }

        float fov = Mathx.FovAtDistanceAndFrustumLength(distanceToGround, viewDiameter);
        return fov;
    }

    public void SetEnabled(bool enabled)
    {
        save.enabled = enabled;
        if (enabled)
        {
            SetZoomIncrement(save.zoomIncrement);
            save.targetCameraState.UpdateTransform(transform);
        }
        else
        {
            SetInitialState();
        }
    }

    public void DoUpdate()
    {
        // Exit Sample
        if (Input.GetKey(KeyCode.Escape) && !TouchInput.IsKeyboardConsumedByUI())
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (!save.enabled)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !TouchInput.IsKeyboardConsumedByUI())
        {
            Rotate(90f);
        }
        if (Input.GetKeyDown(KeyCode.E) && !TouchInput.IsKeyboardConsumedByUI())
        {
            Rotate(-90f);
        }
        if (Input.GetKeyDown(KeyCode.T) && !TouchInput.IsKeyboardConsumedByUI())
        {
            SetZoomIncrement(save.zoomIncrement + 1);
        }
        if (Input.GetKeyDown(KeyCode.G) && !TouchInput.IsKeyboardConsumedByUI())
        {
            SetZoomIncrement(save.zoomIncrement - 1);
        }
        int scrolldelta = Mathf.FloorToInt(Input.mouseScrollDelta.y + accumulatedScroll) - Mathf.FloorToInt(accumulatedScroll);
        accumulatedScroll += Input.mouseScrollDelta.y;
        if (scrolldelta != 0)
        {
            SetZoomIncrement(save.zoomIncrement - scrolldelta);
        }

        // Translation
        Vector3 translation = GetKeyboardTranslationDirection() * panSpeed * GameTime.deltaTime;
        // Speed up movement when shift key held
        if (Input.GetKey(KeyCode.LeftShift))
        {
            translation *= 2.2f;
        }

        save.targetCameraState.TranslateLocal(translation);

        currentPositionLerpTime = Mathf.Lerp(currentPositionLerpTime, positionLerpTime, GameTime.deltaTime);
        currentRotationLerpTime = Mathf.Lerp(currentRotationLerpTime, rotationLerpTime, GameTime.deltaTime);
        currentViewDiameterLerpTime = Mathf.Lerp(currentViewDiameterLerpTime, viewDiameterLerpTime, GameTime.deltaTime);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / currentPositionLerpTime * GameTime.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / currentRotationLerpTime * GameTime.deltaTime);
        var viewDiameterLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / currentViewDiameterLerpTime * GameTime.deltaTime);
        save.interpolatingCameraState.LerpTowards(save.targetCameraState, positionLerpPct, rotationLerpPct, viewDiameterLerpPct);

        save.interpolatingCameraState.UpdateTransform(transform);
        mainCamera.fieldOfView = GetFov(in save.interpolatingCameraState, mainCamera.aspect);
    }
}