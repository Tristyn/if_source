using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour
{

    // Unity UI References
    public RectTransform slider;
    public Text displayText;

    // Create a property to handle the slider's value
    private float currentValue = 0f;
    public float CurrentValue
    {
        get
        {
            return currentValue;
        }
        set
        {
            currentValue = value;
            Vector3 scale = slider.localScale;
            scale.x = Mathf.Clamp01(currentValue);
            slider.localScale = scale;
        }
    }
}