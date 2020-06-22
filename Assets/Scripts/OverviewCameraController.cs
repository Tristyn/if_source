using UnityEngine;

public class OverviewCameraController : MonoBehaviour
{
    class CameraState
    {
        public Vector3 eulerAngles; // Vector3(pitch, yaw, roll)
        public Vector3 position;

        public void SetFromTransform(Transform t)
        {
            eulerAngles = t.eulerAngles;
            position = t.position;
        }

        public void TranslateLocal(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(0, eulerAngles.y, 0) * translation;

            position += rotatedTranslation;
        }

        public void TranslateWorld(Vector3 translation)
        {
            position += translation;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            eulerAngles = Vector3.Lerp(eulerAngles, target.eulerAngles, rotationLerpPct);
            position = Vector3.Lerp(position, target.position, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = eulerAngles;
            t.position = position;
        }
    }

    Camera _mainCamera;
    public Camera mainCamera => _mainCamera ? _mainCamera : _mainCamera = GetComponent<Camera>();

    CameraState targetCameraState = new CameraState();
    CameraState interpolatingCameraState = new CameraState();

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
            targetCameraState.position.y = Mathf.Clamp(targetCameraState.position.y + zoomHeightIncrement, zoomHeightMin, zoomHeightMax);
            if (targetCameraState.position.y == zoomHeightMin)
            {
                targetCameraState.eulerAngles.x = zoomInPitch;
            }
            else
            {
                targetCameraState.eulerAngles.x = pitch;
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            targetCameraState.position.y = Mathf.Clamp(targetCameraState.position.y - zoomHeightIncrement, zoomHeightMin, zoomHeightMax);
            if (targetCameraState.position.y == zoomHeightMin)
            {
                targetCameraState.eulerAngles.x = zoomInPitch;
            }
            else
            {
                targetCameraState.eulerAngles.x = pitch;
            }
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
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / currentPositionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / currentRotationLerpTime) * Time.deltaTime);
        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);

        interpolatingCameraState.UpdateTransform(transform);
    }
}