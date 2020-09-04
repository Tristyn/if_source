using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public struct TouchInfo
{
    public bool valid;
    public bool canvas;
    public bool consumed;

    /// <summary>
    /// Starting position
    /// </summary>
    public Touch press;
    /// <summary>
    /// Current Position
    /// </summary>
    public Touch now;
}

public sealed class TouchInput : Singleton<TouchInput>
{
    /// <summary>
    /// Is touch API supported. It's notably not supported on WebGL,
    /// mobile browsers will use the simulated mouse.
    /// </summary>
    public static bool supported = Application.isMobilePlatform;

    public Action<TouchInfo[]> Touch = touches => { };

    public Func<TouchInfo, bool> IsTouchConsumedByPan;
    public Func<TouchInfo, bool> IsTouchConsumedByPicker;

    public TouchInfo[] touches = new TouchInfo[1];
    List<int> invalidate = new List<int>(1);
    Vector3 lastMousePosition;

    protected override void Awake()
    {
        base.Awake();
        Input.simulateMouseWithTouches = false;
        enabled = supported;
    }

    void Update()
    {
        InputTouch();
    }

    void InputTouch()
    {
        int validTouches = 0;
        for (int i = 0, len = Input.touchCount; i < len; ++i)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                ++validTouches;
                AddTouch(touch);
            }
            else
            {
                bool touchFound = false;
                for (int j = 0, len2 = touches.Length; j < len2; ++j)
                {
                    if (!touches[j].valid)
                    {
                        continue;
                    }

                    if (touches[j].press.fingerId == touch.fingerId)
                    {
                        ++validTouches;
                        touchFound = true;
                        touches[j].now = touch;

                        if (!touches[j].consumed)
                        {
                            Consume(ref touches[j]);
                        }

                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            invalidate.Add(j);
                        }
                        break;
                    }
                }
                if (!touchFound)
                {
                    // WebGL touch can insert new touches that do not start in the 'began' phase
                    ++validTouches;
                    AddTouch(touch);
                }
            }
        }

        Touch(touches);

        for (int i = 0, len = invalidate.Count; i < len; ++i)
        {
            touches[invalidate[i]].valid = false;
        }
        invalidate.Clear();

        // Trim array if at least 3 invalid touches
        if (validTouches + 3 <= touches.Length)
        {
            TouchInfo[] newTouches = new TouchInfo[validTouches];
            for (int i = 0, newTouchesIndex = -1, len = touches.Length; i < len; ++i)
            {
                if (touches[i].valid)
                {
                    newTouches[++newTouchesIndex] = touches[i];
                }
            }
            touches = newTouches;
        }
    }

    void AddTouch(Touch touch)
    {
        bool added = false;
        touch.phase = TouchPhase.Began;
        for (int j = 0, len2 = touches.Length; j < len2; ++j)
        {
            if (!touches[j].valid)
            {
                touches[j] = new TouchInfo
                {
                    valid = true,
                    canvas = EventSystem.current.IsPointerOverGameObject(touch.fingerId),
                    press = touch,
                    now = touch
                };
                Consume(ref touches[j]);
                added = true;
                break;
            }
        }
        if (!added)
        {
            var newTouches = new TouchInfo[touches.Length + 1];
            Array.Copy(touches, newTouches, touches.Length);
            touches = newTouches;

            newTouches[newTouches.Length - 1] = new TouchInfo
            {
                valid = true,
                canvas = EventSystem.current.IsPointerOverGameObject(touch.fingerId),
                press = touch,
                now = touch
            };
            Consume(ref newTouches[newTouches.Length - 1]);
        }
    }

    void TouchWebGLMobile()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Touch touch = new Touch
            {
                phase = TouchPhase.Began,
                position = Input.mousePosition,
                rawPosition = Input.mousePosition,
                deltaPosition = Vector2.zero
            };

            TouchInfo webGLTouchInfo = new TouchInfo
            {
                valid = true,
                canvas = IsPointerConsumedByUI(),
                press = touch,
                now = touch,
            };
            Consume(ref webGLTouchInfo);
            touches[0] = webGLTouchInfo;
            Touch(touches);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            TouchInfo webGLTouchInfo = touches[0];
            webGLTouchInfo.now = new Touch
            {
                phase = TouchPhase.Ended,
                position = Input.mousePosition,
                rawPosition = Input.mousePosition,
                deltaPosition = Input.mousePosition - lastMousePosition
            };
            if (!webGLTouchInfo.consumed)
            {
                Consume(ref webGLTouchInfo);
            }
            touches[0] = webGLTouchInfo;
            Touch(touches);
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            TouchInfo webGLTouchInfo = touches[0];
            webGLTouchInfo.now = new Touch
            {
                position = Input.mousePosition,
                rawPosition = Input.mousePosition,
                deltaPosition = Input.mousePosition - lastMousePosition
            };
            if (webGLTouchInfo.now.deltaPosition != Vector2.zero)
            {
                webGLTouchInfo.now.phase = TouchPhase.Moved;
            }
            else
            {
                webGLTouchInfo.now.phase = TouchPhase.Stationary;
            }
            if (!webGLTouchInfo.consumed)
            {
                Consume(ref webGLTouchInfo);
            }
            touches[0] = webGLTouchInfo;
            Touch(touches);
        }

        lastMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// Consuming a touch is when a particular script takes ownership of the touch.
    /// Consume is recalled each frame until it is consumed and it is typically consumed in the begin phase.
    /// Each script should keep track of which touches it is taking ownership of.
    /// </summary>
    /// <param name="touch">An unconsumed touch</param>
    void Consume(ref TouchInfo touch)
    {
        if (touch.consumed = IsTouchConsumedByPan?.Invoke(touch) == true)
        {
            return;
        }
        if (touch.consumed = IsTouchConsumedByPicker?.Invoke(touch) == true)
        {
            return;
        }
    }

    /// <summary>
    /// Returns true if mouse or touch pointer is consumed by either Canvas or the legacy GUI system.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointerConsumedByUI()
    {
        return EventSystem.current.IsPointerOverGameObject() || GUIUtility.hotControl != 0;
    }
}
