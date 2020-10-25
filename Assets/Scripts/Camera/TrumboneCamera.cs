using UnityEngine;

public class TrumboneCamera : MonoBehaviour
{
    public Transform target;
    public float targetFrustumDiameter = 10;

    void Update()
    {
        Camera camera = GetComponent<Camera>();

        if (!camera || !target)
        {
            return;
        }

        float distance = Vector3.Distance(camera.transform.position, target.position);

        // Calculate based on the smaller of frustum width and height
        float frustumDiameter = targetFrustumDiameter;
        float aspect = camera.aspect;
        if (aspect > 1)
        {
            // Landscape screen, calculate vertical frustum
            frustumDiameter *= aspect;
        }

        float fov = Mathx.FovAtDistanceAndFrustumLength(distance, frustumDiameter);
        camera.fieldOfView = fov;
        camera.transform.LookAt(target);
    }
}