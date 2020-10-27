using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMachineDescription : MonoBehaviour
{
    public Color colorText;
    public Color colorEfficiency;
    public Color colorPrice;

    public UnityEvent onTextAvailable;
    public UnityEvent onTextUnavailable;

    string colorTextHex;
    string colorEfficiencyHex;
    string colorPriceHex;

    SelectionState selectionState;

    TextMeshProUGUI text;
    StringBuilder sb = new StringBuilder(64);
    StringBuilder sbInputs = new StringBuilder(32);

    void Awake()
    {
        colorTextHex = ColorUtility.ToHtmlStringRGB(colorText);
        colorEfficiencyHex = ColorUtility.ToHtmlStringRGB(colorEfficiency);
        colorPriceHex = ColorUtility.ToHtmlStringRGB(colorPrice);
        text = GetComponent<TextMeshProUGUI>();
        Events.SelectionChanged += SelectionChanged;
        UpdateDescribing();
    }

    void OnDestroy()
    {
        Events.InterfaceSelectionChanged -= SelectionChanged;
    }

    void OnValidate()
    {
        colorTextHex = ColorUtility.ToHtmlStringRGB(colorText);
        colorEfficiencyHex = ColorUtility.ToHtmlStringRGB(colorEfficiency);
        colorPriceHex = ColorUtility.ToHtmlStringRGB(colorPrice);
    }

    void SelectionChanged(SelectionState selectionState)
    {
        this.selectionState = selectionState;
        UpdateDescribing();
    }

    void UpdateDescribing()
    {
        if (selectionState != null && selectionState.selectionMode != SelectionMode.None)
        {
            sb.Clear();
            GetText(selectionState);
            text.SetText(sb);
            onTextAvailable.Invoke();
        }
        else
        {
            onTextUnavailable.Invoke();
        }
    }

    void GetText(SelectionState selectionState)
    {
        if (selectionState.selectionMode == SelectionMode.Conveyor)
        {
            sb.Append("<color=#");
            sb.Append(colorTextHex);
            sb.AppendLine("><b>Conveyor</b>");
            sb.Append("<color=#");
            sb.Append(colorPriceHex);
            sb.Append(">Price: </b>$");
            sb.Append(CurrencySystem.instance.conveyorCost);
            sb.Append("</b><color=#");
            sb.Append(colorTextHex);
            sb.AppendLine(">");
            sb.AppendFormat("Transfers items from one machine to another. Transfers up to <b>{0}</b> items per second.", Conveyor.throughput.ToString("#"));
        }
        else if (selectionState.selectionMode == SelectionMode.Machine && selectionState.machineInfo)
        {
            MachineInfo machineInfo = selectionState.machineInfo;
            Machine machine = selectionState.machine;
            sb.Append("<color=#");
            sb.Append(colorTextHex);
            sb.Append("><b>");
            sb.Append(machineInfo.name);
            sb.AppendLine("</b>");
            if (machine)
            {
                sb.Append("<color=#");
                sb.Append(colorTextHex);
                sb.Append(">");
                sb.Append("Efficiency: <b>");
                sb.Append("<color=#");
                sb.Append(colorEfficiencyHex);
                sb.Append(">");
                sb.Append(Mathf.RoundToInt(machine.machineEfficiency.efficiency * 100));
                sb.Append("%</b>");
                sb.AppendLine();
            }
            else
            {
                sb.Append("<color=#");
                sb.Append(colorTextHex);
                sb.Append(">Price: <color=#");
                sb.Append(colorPriceHex);
                sb.Append("><b>$");
                sb.Append(machineInfo.cost);
                sb.Append("</b>");
                sb.AppendLine();
            }
            if (machineInfo.purchaseItem.itemInfo)
            {

                string cost = machineInfo.purchaseItem.itemInfo.value == 0 ? "" :
                    string.Format("for <color=#{0}><b>${1}</b><color=#{2}>",
                        colorPriceHex,
                        machineInfo.purchaseItem.itemInfo.value * machineInfo.purchaseItem.count,
                        colorTextHex);

                sb.AppendFormat("<color=#{0}>Buys <color=#{1}><b>{2}</b> {3}<color=#{4}> {5} every <b>{6}</b> {7} and outputs it to conveyors.",
                    colorTextHex,
                    machineInfo.purchaseItem.itemInfo.colorHex,
                    machineInfo.purchaseItem.count,
                    machineInfo.purchaseItem.itemInfo.itemName,
                    colorTextHex,
                    cost,
                    machineInfo.placeInterval,
                    Mathf.Approximately(machineInfo.placeInterval, 1f) ? "second" : "seconds");
            }
            else if (machineInfo.sellItem.itemInfo)
            {
                sb.AppendFormat("<color=#{0}>Sells <color=#{1}><b>{2}</b> {3}<color=#{4}> for <color=#{5}><b>${6}</b><color=#{7}> every <b>{8}</b> {9}.",
                    colorTextHex,
                    machineInfo.sellItem.itemInfo.colorHex,
                    machineInfo.sellItem.count,
                    machineInfo.sellItem.itemInfo.itemName,
                    colorTextHex,
                    colorPriceHex,
                    machineInfo.sellItem.itemInfo.value * machineInfo.sellItem.count,
                    colorTextHex,
                    machineInfo.placeInterval,
                    Mathf.Approximately(machineInfo.placeInterval, 1f) ? "second" : "seconds");
            }
            else if (machineInfo.assembler)
            {
                sbInputs.Clear();
                for (int i = 0, len = machineInfo.assembleInputs.Length; i < len; ++i)
                {
                    if (i > 0)
                    {
                        if (i != len - 1)
                        {
                            sbInputs.Append(", ");
                        }
                        else
                        {
                            sbInputs.Append(" and ");
                        }
                    }
                    sbInputs.Append("<color=#");
                    sbInputs.Append(machineInfo.assembleInputs[i].itemInfo.colorHex);
                    sbInputs.Append("><b>");
                    sbInputs.Append(machineInfo.assembleInputs[i].count);
                    sbInputs.Append("</b> ");
                    sbInputs.Append(machineInfo.assembleInputs[i].itemInfo.itemNameLower);
                    sbInputs.Append("<color=#");
                    sbInputs.Append(colorTextHex);
                    sbInputs.Append(">");
                }
                sb.AppendFormat("<color=#{0}>Consumes {1} to produce <color=#{2}><b>{3}</b> {4}<color=#{5}> every <b>{6}</b> {7} and outputs it to conveyors.",
                    colorTextHex,
                    sbInputs,
                    machineInfo.assembleOutput.itemInfo.colorHex,
                    machineInfo.assembleOutput.count,
                    machineInfo.assembleOutput.itemInfo.itemName,
                    colorTextHex,
                    machineInfo.placeInterval,
                    Mathf.Approximately(machineInfo.placeInterval, 1f) ? "second" : "seconds");
            }

            if (selectionState.machine && selectionState.machine.inventory.slots.Length > 0)
            {
                InventorySlot[] inventory = selectionState.machine.inventory.slots;
                sb.AppendLine();
                sb.AppendFormat("<b>Inventory</b>");
                for (int i = 0, len = inventory.Length; i < len; ++i)
                {
                    InventorySlot slot = inventory[i];
                    sb.Append(" <color=#");
                    sb.Append(slot.itemInfo.colorHex);
                    sb.Append('>');
                    sb.Append(slot.itemInfo.itemName);
                    sb.Append(":<color=#");
                    sb.Append(colorTextHex);
                    sb.Append("> <b>");
                    sb.Append(slot.count);
                    sb.Append("</b>/");
                    sb.Append(slot.capacity);
                }
            }
        }
    }
}
