using UnityEngine;
using UnityEngine.UI;

public class UISelectMachineButton : MonoBehaviour
{
    public Sprite spritePurchaser;
    public Sprite spriteSeller;
    public Sprite spriteAssembler;
    public Sprite spriteConveyor;

    public Image machineImage;
    public Image machineTypeIcon;
    public Text machineName;

    public bool isConveyor;
    public MachineInfo machineInfo;

    public void Initialize()
    {
        if (isConveyor)
        {
            machineImage.sprite = spriteConveyor;
            machineImage.color = Color.white;
            machineName.text = "Conveyor";
        }
        else if (machineInfo)
        {
            machineImage.sprite = machineInfo.sprite;
            machineImage.color = machineInfo.spriteColor;
            machineName.text = machineInfo.machineName;

            if (machineInfo.assembler)
            {
                machineTypeIcon.sprite = spriteAssembler;
            }
            else if (machineInfo.purchaseItem.itemInfo != null)
            {
                machineTypeIcon.sprite = spritePurchaser;
            }
            else if (machineInfo.sellItem.itemInfo != null)
            {
                machineTypeIcon.sprite = spriteSeller;
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