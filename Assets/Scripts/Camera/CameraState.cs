using System;
using UnityEngine;

[Serializable]
public struct CameraState
{
    public Vector3 eulerAngles; // Vector3(pitch, yaw, roll)
    public Vector3 position;
    public float viewDiameter;

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

    public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct, float viewDiameterLerpPct)
    {
        eulerAngles = Vector3.Lerp(eulerAngles, target.eulerAngles, rotationLerpPct);
        position = Vector3.Lerp(position, target.position, positionLerpPct);
        viewDiameter = Mathf.Lerp(viewDiameter, target.viewDiameter, viewDiameterLerpPct);
    }

    public void UpdateTransform(Transform t)
    {
        t.eulerAngles = eulerAngles;
        t.position = position;
    }

    public void UpdateTransformLocal(Transform t)
    {
        t.localEulerAngles = eulerAngles;
        t.localPosition = position;
    }

    public Ray CameraCenterToRay()
    {
        return new Ray(position, Quaternion.Euler(eulerAngles) * Vector3.forward); 
    }
}