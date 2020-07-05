using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISelectMachineButton : MonoBehaviour
{
    public Sprite spritePurchaser;
    public Sprite spriteSeller;
    public Sprite spriteAssembler;
    public Sprite spriteConveyor;

    public Image machineImage;
    public Image machineCategory;
    public TextMeshProUGUI machineName;

    public bool isConveyor;
    public MachineInfo machineInfo;

    public void Initialize()
    {
        if (isConveyor)
        {
            machineImage.sprite = spriteConveyor;
            machineImage.color = Color.white;
            machineName.text = "Conveyor";
            machineCategory.enabled = false;
        }
        else if (machineInfo)
        {
            machineImage.sprite = machineInfo.sprite;
            machineImage.color = machineInfo.spriteColor;
            machineName.text = machineInfo.machineName;

            if (machineInfo.assembler)
            {
                machineCategory.sprite = spriteAssembler;
                machineCategory.enabled = true;
            }
            else if (machineInfo.purchaseItem.itemInfo != null)
            {
                machineCategory.sprite = spritePurchaser;
                machineCategory.enabled = true;
            }
            else if (machineInfo.sellItem.itemInfo != null)
            {
                machineCategory.sprite = spriteSeller;
                machineCategory.enabled = true;
            }
        }
    }

    public void OnClick()
    {
        if (isConveyor)
        {
            InterfaceSelectionManager.instance.SetSelectionConveyor();
        }
        else if (machineInfo)
        {
            InterfaceSelectionManager.instance.SetSelection(machineInfo);
        }
    }
}