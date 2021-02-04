using UnityEngine;

public sealed class Updater : MonoBehaviour
{
    void Awake()
    {
        Init.Bind += Bind;
    }

    void Bind()
    {
        Entities.backgroundMusic = BackgroundMusic.instance;
        Entities.touchInput = TouchInput.instance;
        Entities.picker = Picker.instance;
        Entities.autoSaveLoad = AutoSaveLoad.instance;

        Entities.overviewCameraController = OverviewCameraController.instance;
        Entities.cameraShake = CameraShake.instance;

        Entities.puzzleGoals = PuzzleGoals.instance;
    }

    void Update()
    {
        GameTime.DoUpdate();
        Entities.autoSaveLoad.DoUpdate();

        Entities.backgroundMusic.DoUpdate();
        Entities.machineDroppers.DoUpdate();

        // Camera transform stack
        Entities.overviewCameraController.DoUpdate();
        Entities.cameraShake.DoUpdate();

        // Rely on camera position
        Entities.linkConveyorButtons.DoUpdate();
        Entities.picker.DoUpdate();
        Entities.touchInput.DoUpdate();
    }

    void FixedUpdate()
    {
        GameTime.DoFixedUpdate();

        Entities.conveyors.DoFixedUpdate();
        Entities.machinePurchasers.DoFixedUpdate();
        Entities.machineSellers.DoFixedUpdate();
        Entities.machineAssemblers.DoFixedUpdate();
        Entities.machinePlacers.DoFixedUpdate();
        Entities.uiSelectMachineButtons.DoFixedUpdate();
        Entities.puzzleGoals.DoFixedUpdate();
        Entities.cameraShake.DoFixedUpdate();
    }
}