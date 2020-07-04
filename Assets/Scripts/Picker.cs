using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PickMask
{
    Floor,
    Construct,
    Demolish
}

public class Picker : Singleton<Picker>
{
    public class TouchPick
    {
        public int fingerId;
        public Vector3Int startingTile;
    }

    public Camera localCamera;
    public LayerMask floorLayerMask;
    public LayerMask constructLayerMask;
    public LayerMask demolishLayerMask;

    private List<TouchPick> touchPicks = new List<TouchPick>(1);
    private bool mouseDragging = false;
    private Vector3Int lastMouseDragPosition;

    protected override void Awake()
    {
        base.Awake();
        if (!localCamera)
        {
            localCamera = GetComponentInChildren<Camera>();
        }

        TouchInput.instance.IsTouchConsumedByPicker = ConsumeTouch;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TouchInput.instance)
        {
            TouchInput.instance.IsTouchConsumedByPicker = null;
        }
    }

    void Update()
    {
        InputDesktop();
    }

    void InputDesktop()
    {
        InterfaceState interfaceState = InterfaceSelectionManager.instance.state;
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (interfaceState.mode == InterfaceMode.Conveyor)
            {
                if (GetPickerTile(Input.mousePosition, PickMask.Construct, out Vector3Int pickerPosition))
                {
                    mouseDragging = true;
                    lastMouseDragPosition = pickerPosition;
                    Conveyor conveyor = Conveyor.CreateConveyor(pickerPosition);
                    if (conveyor)
                    {
                        TileSelectionManager.instance.SetSelection(conveyor);
                    }
                }
            }
        }
        if (Input.GetKey(KeyCode.Mouse0) && mouseDragging)
        {
            if (GetPickerTile(Input.mousePosition, PickMask.Construct, out Vector3Int pickerPosition) && pickerPosition != lastMouseDragPosition)
            {
                if (interfaceState.mode == InterfaceMode.Conveyor)
                {
                    Conveyor conveyor;
                    if ((pickerPosition - lastMouseDragPosition).ToDirection() != Directions.None)
                    {
                        conveyor = Conveyor.CreateConveyor(lastMouseDragPosition, pickerPosition);
                    }
                    else
                    {
                        conveyor = Conveyor.CreateConveyor(pickerPosition);
                    }
                    if (conveyor)
                    {
                        TileSelectionManager.instance.SetSelection(conveyor);
                    }
                    lastMouseDragPosition = pickerPosition;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            mouseDragging = false;
            if (interfaceState.mode == InterfaceMode.Machine && !EventSystem.current.IsPointerOverGameObject())
            {
                if (GetPickerPosition(Input.mousePosition, PickMask.Construct, out Vector3 pickerPosition))
                {
                    Machine.CreateMachine(interfaceState.machineInfo, pickerPosition);
                }
            }
        }

        if (Input.GetKey(KeyCode.Mouse2) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (GetPickerTile(Input.mousePosition, PickMask.Demolish, out Vector3Int pickerPosition))
            {
                if (ConveyorSystem.instance.conveyors.TryGetValue(pickerPosition, out Conveyor conveyor))
                {
                    if (TileSelectionManager.instance.state.conveyor == conveyor)
                    {
                        TileSelectionManager.instance.SelectInput(false);
                    }
                    conveyor.PlayDemolishAudio();
                    conveyor.Recycle();
                }
            }
        }
    }

    bool ConsumeTouch(TouchInfo touch)
    {
        if (touch.now.phase == TouchPhase.Began && !touch.canvas && GetPickerTile(touch.now.position, PickMask.Construct, out Vector3Int pickerPosition))
        {
            touchPicks.Add(new TouchPick
            {
                fingerId = touch.now.fingerId,
                startingTile = pickerPosition
            });
            return false;
        }

        if (touch.now.phase == TouchPhase.Moved)
        {
            if (TouchPicksContain(touch.now.fingerId, out int touchPickIndex) && GetPickerTile(touch.now.position, PickMask.Construct, out pickerPosition))
            {
                TouchPick touchPick = touchPicks[touchPickIndex];
                if (pickerPosition != touchPick.startingTile)
                {
                    RemoveTouchPick(touch.now.fingerId);
                    return false;
                }
            }
        }

        if (touch.now.phase == TouchPhase.Ended)
        {
            if (TouchPicksContain(touch.now.fingerId, out int touchPickIndex) && GetPickerTile(touch.now.position, PickMask.Construct, out pickerPosition) && touchPicks[touchPickIndex].startingTile == pickerPosition)
            {
                RemoveTouchPick(touch.now.fingerId);
                // If we selected a neighbor and tapped here it might be because we mistapped a ConveyorButton
                // Link them anyway like a conveyor button
                Conveyor conveyor = null;
                if (TileSelectionManager.instance.state.bounds.HasValue)
                {
                    foreach ((Vector3Int outerTile, Vector3Int innerTile) in TileSelectionManager.instance.state.bounds.Value.EnumeratePerimeter())
                    {
                        if (outerTile == pickerPosition)
                        {
                            conveyor = Conveyor.CreateConveyor(innerTile, pickerPosition);
                            TileSelectionManager.instance.PanTo(conveyor);
                            break;
                        }
                    }
                }
                if (!conveyor)
                {
                    conveyor = Conveyor.CreateConveyor(pickerPosition);
                }
                if (conveyor)
                {
                    TileSelectionManager.instance.SetSelection(conveyor);
                }
                return true;
            }
        }

        if (touch.now.phase == TouchPhase.Canceled)
        {
            RemoveTouchPick(touch.now.fingerId);
        }
        return false;
    }

    bool TouchPicksContain(int fingerId, out int touchPickIndex)
    {
        for (int i = 0, len = touchPicks.Count; i < len; i++)
        {
            if (touchPicks[i].fingerId == fingerId)
            {
                touchPickIndex = i;
                return true;
            }
        }
        touchPickIndex = -1;
        return false;
    }

    bool RemoveTouchPick(int fingerId)
    {
        for (int i = 0, len = touchPicks.Count; i < len; i++)
        {
            if (touchPicks[i].fingerId == fingerId)
            {
                touchPicks.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool GetPickerTile(Vector3 pixelCoordinates, PickMask pickMask, out Vector3Int pickerTile)
    {
        LayerMask layerMask;
        switch (pickMask)
        {
            case PickMask.Floor:
                layerMask = constructLayerMask;
                break;
            case PickMask.Construct:
                layerMask = constructLayerMask;
                break;
            case PickMask.Demolish:
                layerMask = demolishLayerMask;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (Physics.Raycast(localCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 100, layerMask))
        {
            pickerTile = hitInfo.point.RoundToTile();
            pickerTile.y = 0;
            return true;
        }
        pickerTile = Vector3Int.zero;
        return false;
    }

    public bool GetPickerPosition(Vector3 pixelCoordinates, PickMask pickMask, out Vector3 pickerPosition)
    {
        LayerMask layerMask;
        switch (pickMask)
        {
            case PickMask.Floor:
                layerMask = constructLayerMask;
                break;
            case PickMask.Construct:
                layerMask = constructLayerMask;
                break;
            case PickMask.Demolish:
                layerMask = demolishLayerMask;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (Physics.Raycast(localCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 100, layerMask))
        {
            pickerPosition = hitInfo.point;
            return true;
        }
        pickerPosition = Vector3.zero;
        return false;
    }


}
