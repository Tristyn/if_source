using System.Collections.Generic;
using UnityEngine;

public sealed class TouchPan : MonoBehaviour
{
    private struct Pan
    {
        public int fingerId;
        public Vector3Int startingTile;
        public Vector2 startingPixelCoordinate;
        public bool panningBegan;
    }

    List<Pan> pans = new List<Pan>(1);

    void Awake()
    {
        Init.Bind += () =>
        {
            TouchInput.instance.IsTouchConsumedByPan = ConsumeTouch;
            TouchInput.instance.Touch += OnTouch;
        };
    }

    void OnDestroy()
    {
        if (TouchInput.instance)
        {
            TouchInput.instance.IsTouchConsumedByPan = null;
            TouchInput.instance.Touch -= OnTouch;
        }
    }

    bool ConsumeTouch(TouchInfo touch)
    {
        if (touch.now.phase == TouchPhase.Began && !touch.canvas && Picker.instance.GetPickerTile(touch.now.position, PickMask.Floor, out Vector3Int pickerPosition))
        {
            pans.Add(new Pan
            {
                fingerId = touch.now.fingerId,
                startingTile = pickerPosition,
                startingPixelCoordinate = touch.now.position
            });
            return false;
        }

        if (touch.now.phase == TouchPhase.Moved)
        {
            for (int i = 0, len = pans.Count; i < len; ++i)
            {
                Pan pan = pans[i];
                if (!pan.panningBegan && Picker.instance.GetPickerTile(touch.now.position, PickMask.Floor, out pickerPosition) && pickerPosition != pan.startingTile)
                {
                    pan.panningBegan = true;
                    pans[i] = pan;
                    return true;
                }
            }
        }
        return false;
    }

    void OnTouch(TouchInfo[] touches)
    {
        for (int i = 0, len = touches.Length; i < len; ++i)
        {
            TouchInfo touch = touches[i];
            if (touch.valid)
            {
                if (touch.now.phase == TouchPhase.Moved || touch.now.phase == TouchPhase.Ended)
                {
                    if (PansContain(touch.now.fingerId, out int panIndex) )
                    {
                        Pan pan = pans[panIndex];
                        if (pan.panningBegan)
                        {
                            CameraPan(touch.now.deltaPosition);
                        }
                        else if(Picker.instance.GetPickerTile(touch.now.position, PickMask.Floor, out Vector3Int pickerPosition) && pickerPosition != pan.startingTile)
                        {
                            pan.panningBegan = true;
                            pans[panIndex] = pan;
                            Vector2 deltaPosition = touch.now.position - pan.startingPixelCoordinate;
                            if(Application.platform == RuntimePlatform.WebGLPlayer)
                            {
                                // coordinates are inverse on WebGL
                                deltaPosition = -deltaPosition;
                            }
                            CameraPan(deltaPosition);
                        }
                    }
                }

                if (touch.now.phase == TouchPhase.Ended || touch.now.phase == TouchPhase.Canceled)
                {
                    RemovePan(touch.now.fingerId);
                }
            }
        }
    }

    void CameraPan(Vector2 translation_local)
    {
        float minScreenDimension = -Mathf.Min(Screen.width, Screen.height);
        Vector3 panAmount = new Vector2(translation_local.x / minScreenDimension, translation_local.y / minScreenDimension);
        OverviewCameraController.instance.PanLocal(panAmount);
    }

    bool PansContain(int fingerId, out int panIndex)
    {
        for(int i =0, len = pans.Count; i < len; ++i)
        {
            if(pans[i].fingerId == fingerId)
            {
                panIndex = i;
                return true;
            }
        }
        panIndex = -1;
        return false;
    }

    bool RemovePan(int fingerId)
    {
        for (int i = 0, len = pans.Count; i < len; ++i)
        {
            if (pans[i].fingerId == fingerId)
            {
                pans.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
}
