﻿using System;
using System.Collections.Generic;
using UnityEngine;

public enum PickMask
{
    Floor,
    Construct,
    Demolish
}

public sealed class Picker : Singleton<Picker>
{
    public struct TouchPick
    {
        public int fingerId;
        public Vector3Int startingTile;
    }

    public LayerMask floorLayerMask;
    public LayerMask constructLayerMask;
    public LayerMask demolishLayerMask;

    Camera mainCamera;
    List<TouchPick> touchPicks = new List<TouchPick>(1);
    bool mouseDragging = false;
    bool mouseDraggingConveyor = false;
    Vector3Int lastMouseDragPosition;

    MachineCreationVisualizer machineCreationVisualizer;

    protected override void Awake()
    {
        base.Awake();
        Init.Bind += () =>
        {
            mainCamera = MainCamera.instance;
            TouchInput.instance.IsTouchConsumedByPicker = ConsumeTouch;
            machineCreationVisualizer = ObjectPooler.instance.Get<MachineCreationVisualizer>();
        };
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TouchInput.instance)
        {
            TouchInput.instance.IsTouchConsumedByPicker = null;
        }
    }

    public void DoUpdate()
    {
        InputDesktop();
    }

    void InputDesktop()
    {
        Vector3 pickerPosition;
        Vector3Int pickerTile;

        SelectionState interfaceState = InterfaceSelectionManager.instance.state;
        if (Input.GetKeyDown(KeyCode.Mouse0) && !TouchInput.IsPointerConsumedByUI())
        {
            TouchInput.inputMode = InputMode.Mouse;
            if (GetPickerTile(Input.mousePosition, PickMask.Construct, out pickerTile))
            {
                mouseDragging = true;
                lastMouseDragPosition = pickerTile;
                if (!TileSelectionManager.instance.SetSelection(pickerTile))
                {
                    if (interfaceState.selectionMode == SelectionMode.Conveyor)
                    {
                        mouseDraggingConveyor = true;
                        if (!ConveyorSystem.instance.conveyors.TryGetValue(pickerTile, out Conveyor conveyor))
                        {
                            conveyor = ConveyorSystem.instance.CreateConveyor(pickerTile, ConveyorCreateFlags.None);
                            if (conveyor)
                            {
                                TileSelectionManager.instance.SetSelection(conveyor);
                            }
                        }
                    }
                }
            }
        }
        if (Input.GetKey(KeyCode.Mouse0) && mouseDragging)
        {
            TouchInput.inputMode = InputMode.Mouse;
            if (GetPickerTile(Input.mousePosition, PickMask.Construct, out pickerTile) && pickerTile != lastMouseDragPosition)
            {
                Conveyor conveyor = null;
                if (lastMouseDragPosition.IsNeighbor(pickerTile))
                {
                    mouseDraggingConveyor = true;
                    conveyor = ConveyorSystem.instance.GetOrCreateConveyor(lastMouseDragPosition, pickerTile, ConveyorCreateFlags.SelectConveyor);
                }
                else
                {
                    conveyor = ConveyorSystem.instance.CreateConveyor(pickerTile, ConveyorCreateFlags.SelectConveyor);
                }
                lastMouseDragPosition = pickerTile;
            }
        }
        Pickable pickable;
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            TouchInput.inputMode = InputMode.Mouse;
            if (!mouseDraggingConveyor && interfaceState.selectionMode == SelectionMode.Machine && !TouchInput.IsPointerConsumedByUI())
            {
                if (GetPickerPosition(Input.mousePosition, PickMask.Construct, out pickerPosition, out pickable))
                {
                    Bounds3Int bounds = pickerPosition.PositionBottomToBounds(interfaceState.machineInfo.size);
                    Machine machine = MachineSystem.instance.CreateMachine(interfaceState.machineInfo, bounds);
                    if (machine)
                    {
                        machine.Drop();
                        TileSelectionManager.instance.SetSelection(machine);
                    }
                }
            }
            mouseDragging = false;
            mouseDraggingConveyor = false;
        }

        OnClick(Input.GetKeyUp(KeyCode.Mouse0));

        if (interfaceState.selectionMode == SelectionMode.Machine &&
            GetPickerPosition(Input.mousePosition, PickMask.Construct, out pickerPosition, out pickable))
        {
            TouchInput.inputMode = InputMode.Mouse;
            machineCreationVisualizer.Visualize(interfaceState.machineInfo, pickerPosition);
            machineCreationVisualizer.SetVisible(true);
        }
        else
        {
            machineCreationVisualizer.SetVisible(false);
        }

        if (Input.GetKey(KeyCode.Mouse1) && !TouchInput.IsPointerConsumedByUI())
        {
            TouchInput.inputMode = InputMode.Mouse;
            if (GetPickerTile(Input.mousePosition, PickMask.Demolish, out pickerTile))
            {
                Machine machine = MachineSystem.instance.GetMachine(pickerTile);
                if (machine)
                {
                    if (TileSelectionManager.instance.selectionState.machine == machine)
                    {
                        TileSelectionManager.instance.TrySelectAnyInput(false);
                    }
                    machine.Demolish();
                }
                else if (ConveyorSystem.instance.conveyors.TryGetValue(pickerTile, out Conveyor conveyor))
                {
                    if (TileSelectionManager.instance.selectionState.conveyor == conveyor)
                    {
                        TileSelectionManager.instance.TrySelectAnyInput(false);
                    }
                    conveyor.Demolish();
                }
            }
        }
    }

    bool ConsumeTouch(TouchInfo touch)
    {
        return OnClick(touch);
    }

    bool TouchPicksContain(int fingerId, out int touchPickIndex)
    {
        for (int i = 0, len = touchPicks.Count; i < len; ++i)
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
        for (int i = 0, len = touchPicks.Count; i < len; ++i)
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 1000, layerMask))
        {
            pickerTile = hitInfo.point.RoundDown();
            pickerTile.y = 0;
            return true;
        }
        pickerTile = Vector3Int.zero;
        return false;
    }

    public bool GetPickerPosition(Vector3 pixelCoordinates, PickMask pickMask, out Vector3 pickerPosition, out Pickable pickable)
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 1000, layerMask))
        {
            pickerPosition = hitInfo.point;
            pickable = hitInfo.collider.gameObject.GetComponent<Pickable>();
            return true;
        }
        pickerPosition = Vector3.zero;
        pickable = null;
        return false;
    }


    bool OnClick(bool clicked)
    {
        Pickable pickable = GetClickTarget();

        TouchInfo touch = new TouchInfo
        {
            valid = true,
            canvas = TouchInput.IsPointerConsumedByUI(),
            press = new Touch
            {
                phase = TouchPhase.Began,
                position = Input.mousePosition,
                rawPosition = Input.mousePosition
            },
            now = new Touch
            {
                phase = clicked ? TouchPhase.Moved : TouchPhase.Ended,
                position = Input.mousePosition,
                rawPosition = Input.mousePosition
            }
        };

        return TargetClicked(pickable, touch);
    }


    bool OnClick(TouchInfo touch)
    {
        Pickable pickable = GetClickTarget();
        return TargetClicked(pickable, touch);
    }

    Pickable GetClickTarget()
    {
        GetPickerPosition(Input.mousePosition, PickMask.Construct, out Vector3 pickerPosition, out Pickable pickable);
        return pickable;
    }
    Pickable lastPickableClicked;
    bool TargetClicked(Pickable pickable, TouchInfo touch)
    {
        if (lastPickableClicked && lastPickableClicked != pickable)
        {
            lastPickableClicked.state = default;
            lastPickableClicked.picked.Invoke();
        }

        if (ConsumeTouchMachineConveyor(touch))
        {
            lastPickableClicked = null;
            return true;
        }

        lastPickableClicked = pickable;
        if (pickable != null)
        {
            pickable.state = new PickableTriggerState
            {
                mouseHovered = true,
                mouseClicked = touch.now.phase == TouchPhase.Ended
            };
            pickable.picked.Invoke();

            return true;
        }
        return false;
    }

    bool ConsumeTouchMachineConveyor(TouchInfo touch)
    {
        if (!touch.canvas)
        {
            if (touch.now.phase == TouchPhase.Began && GetPickerTile(touch.now.position, PickMask.Construct, out Vector3Int pickerTile))
            {
                TouchInput.inputMode = InputMode.Touch;
                touchPicks.Add(new TouchPick
                {
                    fingerId = touch.now.fingerId,
                    startingTile = pickerTile
                });
                return false;
            }

            if (touch.now.phase == TouchPhase.Moved)
            {
                if (TouchPicksContain(touch.now.fingerId, out int touchPickIndex) && GetPickerTile(touch.now.position, PickMask.Construct, out pickerTile))
                {
                    TouchPick touchPick = touchPicks[touchPickIndex];
                    if (pickerTile != touchPick.startingTile)
                    {
                        RemoveTouchPick(touch.now.fingerId);
                        return false;
                    }
                }
            }

            if (touch.now.phase == TouchPhase.Ended)
            {
                if (TouchPicksContain(touch.now.fingerId, out int touchPickIndex))
                {
                    TouchPick touchPick = touchPicks[touchPickIndex];
                    RemoveTouchPick(touch.now.fingerId);
                    return DoTouchMachineConveyor(touch, touchPick);
                }
            }
        }
        RemoveTouchPick(touch.now.fingerId);
        return false;
    }

    public bool DoTouchMachineConveyor(TouchInfo touch, TouchPick touchPick)
    {
        if (GetPickerPosition(touch.now.position, PickMask.Construct, out Vector3 pickerPosition, out Pickable pickable) && touchPick.startingTile == pickerPosition.RoundDown())
        {
            SelectionState interfaceState = InterfaceSelectionManager.instance.state;
            if (interfaceState.selectionMode == SelectionMode.Machine)
            {
                Machine machine = MachineSystem.instance.GetMachine(pickerPosition.RoundDown());
                if (!machine)
                {
                    Bounds3Int bounds = pickerPosition.PositionBottomToBounds(interfaceState.machineInfo.size);
                    machine = MachineSystem.instance.CreateMachine(interfaceState.machineInfo, bounds);
                }
                if (machine)
                {
                    machine.Drop();
                    TileSelectionManager.instance.SetSelection(machine);
                    return true;
                }
            }
            else if (interfaceState.selectionMode == SelectionMode.Conveyor)
            {
                // If we selected a neighbor and tapped here it might be because we mistapped a ConveyorButton
                // Link them anyway like a conveyor button
                Conveyor conveyor = null;
                SelectionState selection = TileSelectionManager.instance.selectionState;
                if (selection.isSelected)
                {
                    foreach ((Vector3Int outerTile, Vector3Int innerTile) in selection.bounds.EnumeratePerimeter())
                    {
                        if (outerTile == pickerPosition.RoundDown())
                        {
                            conveyor = ConveyorSystem.instance.GetOrCreateConveyor(lastMouseDragPosition, pickerPosition.RoundDown(), ConveyorCreateFlags.SelectConveyor | ConveyorCreateFlags.PanRelative);
                            return true;
                        }
                    }
                }
                if (!conveyor)
                {
                    conveyor = ConveyorSystem.instance.CreateConveyor(pickerPosition.RoundDown(), ConveyorCreateFlags.SelectConveyor);
                    return true;
                }
            }
        }
        return false;
    }

}
