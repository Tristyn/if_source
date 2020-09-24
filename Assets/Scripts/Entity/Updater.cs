using UnityEngine;

public sealed class Updater : MonoBehaviour
{
    static BackgroundMusic backgroundMusic;
    static TouchInput touchInput;
    static Picker picker;
    static AutoSaveLoad autoSaveLoad;

    public static FastRemoveList<UILinkConveyorButton> linkConveyorButtons = FastRemoveList<UILinkConveyorButton>.New(16);
    public static FastRemoveList<MachineDropper> machineDroppers = FastRemoveList<MachineDropper>.New(1);

    static OverviewCameraController overviewCameraController;
    static CameraShake cameraShake;


    public static FastRemoveList<Conveyor> conveyors = FastRemoveList<Conveyor>.New(128);
    public static FastRemoveList<MachinePurchaser> machinePurchasers = FastRemoveList<MachinePurchaser>.New(16);
    public static FastRemoveList<MachineSeller> machineSellers = FastRemoveList<MachineSeller>.New(16);
    public static FastRemoveList<MachineAssembler> machineAssemblers = FastRemoveList<MachineAssembler>.New(16);
    public static FastRemoveList<MachinePlacer> machinePlacers = FastRemoveList<MachinePlacer>.New(32);
    public static CampaignGoals campaignGoals;

    void Awake()
    {
        Init.Bind += Bind;
    }

    void Bind()
    {
        backgroundMusic = BackgroundMusic.instance;
        touchInput = TouchInput.instance;
        picker = Picker.instance;
        autoSaveLoad = AutoSaveLoad.instance;

        overviewCameraController = OverviewCameraController.instance;
        cameraShake = CameraShake.instance;

        campaignGoals = CampaignGoals.instance;
    }

    void Update()
    {
        GameTime.DoUpdate();
        autoSaveLoad.DoUpdate();

        backgroundMusic.DoUpdate();
        machineDroppers.DoUpdate();

        // Camera transform stack
        overviewCameraController.DoUpdate();
        cameraShake.DoUpdate();

        // Rely on camera position
        linkConveyorButtons.DoUpdate();
        picker.DoUpdate();
        touchInput.DoUpdate();
    }

    void FixedUpdate()
    {
        GameTime.DoFixedUpdate();

        conveyors.DoFixedUpdate();
        machinePurchasers.DoFixedUpdate();
        machineSellers.DoFixedUpdate();
        machineAssemblers.DoFixedUpdate();
        machinePlacers.DoFixedUpdate();

        campaignGoals.DoFixedUpdate();
    }
}