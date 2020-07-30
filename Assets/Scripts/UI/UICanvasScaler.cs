using UnityEngine;
using UnityEngine.UI;

public sealed class UICanvasScaler : MonoBehaviour
{
    public float desktopScale;
    
    void OnEnable()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (!Application.isMobilePlatform && !TouchInput.webGLMobile)
        {
            scaler.referenceResolution *= desktopScale;
        }
    }
}
