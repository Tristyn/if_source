using System;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    /* Selection source is prioritized as:
     * -SelectMachineButtonHover
     * -TileHover
     * -The most recent of InterfaceSelection or TileSelection
     */

    SelectionState selectMachineButtonHovered;
    SelectionState tileOrInterfaceSelection;

    public SelectionState selectionState;

    void Awake()
    {
        Events.SelectMachineButtonHovered += SelectMachineButtonHovered;
        Events.TileSelectionChanged += TileSelectionChanged;
        Events.InterfaceSelectionChanged += InterfaceSelectionChanged;
    }

    void OnDestroy()
    {
        Events.SelectMachineButtonHovered -= SelectMachineButtonHovered;
        Events.TileSelectionChanged -= TileSelectionChanged;
        Events.InterfaceSelectionChanged -= InterfaceSelectionChanged;
    }

    void SelectMachineButtonHovered(SelectionState selectionState)
    {
        selectMachineButtonHovered = selectionState;
        SelectionsChanged();
    }

    void TileSelectionChanged(SelectionState selectionState)
    {
        tileOrInterfaceSelection = selectionState;
        SelectionsChanged();
    }

    void InterfaceSelectionChanged(SelectionState selectionState)
    {
        tileOrInterfaceSelection = selectionState;
        SelectionsChanged();
    }

    void SelectionsChanged()
    {
        SelectionState newSelection = selectMachineButtonHovered;
        if (newSelection != null && newSelection.selectionMode == SelectionMode.None)
        {
            newSelection = tileOrInterfaceSelection;
        }

        if (selectionState != newSelection)
        {
            selectionState = newSelection;
            Events.SelectionChanged?.Invoke(selectionState);
        }
    }
}
