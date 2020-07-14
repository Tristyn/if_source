using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;
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

    void Update()
    {
        InputDesktop();
    }

    void InputDesktop()
    {
        Vector3 pickerPosition;
        Vector3Int pickerTile;

        InterfaceState interfaceState = InterfaceSelectionManager.instance.state;
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (GetPickerTile(Input.mousePosition, PickMask.Construct, out pickerTile))
            {
                mouseDragging = true;
                lastMouseDragPosition = pickerTile;
                if (!TileSelectionManager.instance.SetSelection(pickerTile))
                {
                    if (interfaceState.mode == InterfaceMode.Conveyor)
                    {
                        mouseDraggingConveyor = true;
                        Conveyor conveyor = Conveyor.CreateConveyor(pickerTile);
                        if (conveyor)
                        {
                            TileSelectionManager.instance.SetSelection(conveyor);
                        }
                    }
                }
            }
        }
        if (Input.GetKey(KeyCode.Mouse0) && mouseDragging)
        {
            if (GetPickerTile(Input.mousePosition, PickMask.Construct, out pickerTile) && pickerTile != lastMouseDragPosition)
            {
                Conveyor conveyor = null;
                if ((pickerTile - lastMouseDragPosition).ToDirection() != Directions.None)
                {
                    mouseDraggingConveyor = true;
                    if (ConveyorSystem.instance.CanLink(lastMouseDragPosition, pickerTile))
                    {
                        conveyor = Conveyor.CreateConveyor(lastMouseDragPosition, pickerTile);
                        Assert.IsNotNull(conveyor);
                    }
                }
                else
                {
                    if (ConveyorSystem.instance.CanCreate(pickerTile))
                    {
                        conveyor = Conveyor.CreateConveyor(pickerTile);
                        Assert.IsNotNull(conveyor);
                    }
                }
                if (conveyor)
                {
                    TileSelectionManager.instance.SetSelection(conveyor);
                }
                lastMouseDragPosition = pickerTile;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (!mouseDraggingConveyor && interfaceState.mode == InterfaceMode.Machine && !EventSystem.current.IsPointerOverGameObject())
            {
                if (GetPickerPosition(Input.mousePosition, PickMask.Construct, out pickerPosition))
                {
                    Machine machine = Machine.CreateMachine(interfaceState.machineInfo, pickerPosition);
                    if (machine)
                    {
                        TileSelectionManager.instance.SetSelection(machine);
                        InterfaceSelectionManager.instance.SetSelectionConveyor();
                    }
                }
            }
            mouseDragging = false;
            mouseDraggingConveyor = false;
        }

        if (interfaceState.mode == InterfaceMode.Machine &&
            GetPickerPosition(Input.mousePosition, PickMask.Construct, out pickerPosition))
        {
            machineCreationVisualizer.Visualize(interfaceState.machineInfo, pickerPosition);
            machineCreationVisualizer.SetVisible(true);
        }
        else
        {
            machineCreationVisualizer.SetVisible(false);
        }

        if (Input.GetKey(KeyCode.Mouse2) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (GetPickerTile(Input.mousePosition, PickMask.Demolish, out pickerTile))
            {
                if (MachineSystem.instance.GetMachine(pickerTile, out Machine machine))
                {
                    if (TileSelectionManager.instance.state.machine == machine)
                    {
                        TileSelectionManager.instance.SelectInput(false);
                    }
                    machine.PlayDemolishAudio();
                    machine.Delete();
                }
                else if (ConveyorSystem.instance.conveyors.TryGetValue(pickerTile, out Conveyor conveyor))
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
        if (!touch.canvas)
        {
            if (touch.now.phase == TouchPhase.Began && GetPickerTile(touch.now.position, PickMask.Construct, out Vector3Int pickerPosition))
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
                if (TouchPicksContain(touch.now.fingerId, out int touchPickIndex))
                {
                    TouchPick touchPick = touchPicks[touchPickIndex];
                    RemoveTouchPick(touch.now.fingerId);
                    if (GetPickerTile(touch.now.position, PickMask.Construct, out pickerPosition) && touchPick.startingTile == pickerPosition)
                    {
                        InterfaceState interfaceState = InterfaceSelectionManager.instance.state;
                        if (interfaceState.mode == InterfaceMode.Machine)
                        {
                            if (!MachineSystem.instance.GetMachine(pickerPosition, out Machine machine))
                            {
                                machine = Machine.CreateMachine(interfaceState.machineInfo, pickerPosition);
                            }
                            if (machine)
                            {
                                TileSelectionManager.instance.SetSelection(machine);
                            }
                            return true;
                        }
                        else if (interfaceState.mode == InterfaceMode.Conveyor)
                        {
                            // If we selected a neighbor and tapped here it might be because we mistapped a ConveyorButton
                            // Link them anyway like a conveyor button
                            Conveyor conveyor = null;
                            TileSelectionState selection = TileSelectionManager.instance.state;
                            if (selection.isSelected)
                            {
                                foreach ((Vector3Int outerTile, Vector3Int innerTile) in selection.bounds.EnumeratePerimeter())
                                {
                                    if (outerTile == pickerPosition)
                                    {
                                        conveyor = Conveyor.CreateConveyor(innerTile, pickerPosition);
                                        if (conveyor)
                                        {
                                            Vector3 deltaPosition = pickerPosition - innerTile;
                                            OverviewCameraController.instance.MoveWorld(deltaPosition);
                                        }
                                        break;
                                    }
                                }
                            }
                            if (!conveyor)
                            {
                                conveyor = Conveyor.CreateConveyor(pickerPosition);
                            }
                            TileSelectionManager.instance.SetSelection(pickerPosition);
                            return true;
                        }
                    }
                }
            }

            if (touch.now.phase == TouchPhase.Canceled)
            {
                RemoveTouchPick(touch.now.fingerId);
            }
        }
        return false;
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 100, layerMask))
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(pixelCoordinates), out RaycastHit hitInfo, 100, layerMask))
        {
            pickerPosition = hitInfo.point;
            return true;
        }
        pickerPosition = Vector3.zero;
        return false;
    }
}
