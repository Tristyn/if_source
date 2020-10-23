using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UISelectMachineButton : MonoBehaviour, IFixedUpdate
{
    public Sprite spritePurchaser;
    public Sprite spriteSeller;
    public Sprite spriteAssembler;
    public Sprite spriteConveyor;
    public AudioClip selectMachineClip;

    public Image backgroundImage;
    public Image machineImage;
    public Image machineCategory;
    public TextMeshProUGUI machineName;

    public Color buildableBackgroundColor;
    public Color unbuildableBackgroundColor;

    public bool isConveyor;
    public MachineInfo machineInfo;

    private UIBehaviour[] uiBehaviours;

    bool _visible;

    void Awake()
    {
        uiBehaviours = GetComponentsInChildren<UIBehaviour>();
        Init.LoadComplete += LoadComplete;
        Events.machineUnlocked += OnMachineUnlocked;
    }

    void OnDestroy()
    {
        Init.LoadComplete -= LoadComplete;
        Events.machineUnlocked -= OnMachineUnlocked;
    }
    private void OnEnable()
    {
        Updates.uiSelectMachineButtons.SetAdded(isActiveEnabledAndVisible, this);
        SetBackgroundColor();
    }

    void OnDisable()
    {
        Updates.uiSelectMachineButtons.SetAdded(isActiveEnabledAndVisible, this);
        SetBackgroundColor();
    }

    public void Initialize()
    {
        visible = isConveyor || MachineUnlockSystem.instance.unlocked.Contains(machineInfo);

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
            }
            else if (machineInfo.purchaseItem.itemInfo != null)
            {
                machineCategory.sprite = spritePurchaser;
            }
            else if (machineInfo.sellItem.itemInfo != null)
            {
                machineCategory.sprite = spriteSeller;
            }
        }
    }

    void LoadComplete()
    {
        Initialize();
    }

    void OnMachineUnlocked(MachineInfo machineInfo)
    {
        Initialize();
    }

    public bool isActiveEnabledAndVisible
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _visible && isActiveAndEnabled;
    }

    bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
            for (int i = 0, len = uiBehaviours.Length; i < len; ++i)
            {
                uiBehaviours[i].enabled = value;
            }
            Updates.uiSelectMachineButtons.SetAdded(isActiveEnabledAndVisible, this);
            SetBackgroundColor();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoFixedUpdate()
    {
        SetBackgroundColor();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetBackgroundColor()
    {
        Color backgroundColor;
        CurrencySystem currencySystem = CurrencySystem.instance;
        if (currencySystem && (isConveyor || machineInfo))
        {
            long money = currencySystem.save.money;
            long cost = isConveyor
                ? currencySystem.conveyorCost
                : machineInfo.cost;
            if (money >= cost)
            {
                backgroundColor = buildableBackgroundColor;
            }
            else
            {
                backgroundColor = unbuildableBackgroundColor;
            }
            backgroundImage.color = backgroundColor;
        }
    }

    void SetUI()
    {

    }

    public void OnClick()
    {
        PlaySelectMachineAudio();
        if (isConveyor)
        {
            InterfaceSelectionManager.instance.SetSelectionConveyor();
        }
        else if (machineInfo)
        {
            InterfaceSelectionManager.instance.SetSelection(machineInfo);
        }

        Analytics.instance.NewUiEvent(UiEventId.ButtonSelectMachine, 1);
    }

    public void PlaySelectMachineAudio()
    {
        AudioSystem.instance.PlayOneShot(selectMachineClip, AudioCategory.Effect);
    }
}