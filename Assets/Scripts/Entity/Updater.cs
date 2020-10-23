using UnityEngine;

public sealed class Updater : MonoBehaviour
{

    void Awake()
    {
        Init.Bind += Bind;
    }

    void Bind()
    {
        Updates.backgroundMusic = BackgroundMusic.instance;
        Updates.touchInput = TouchInput.instance;
        Updates.picker = Picker.instance;
        Updates.autoSaveLoad = AutoSaveLoad.instance;

        Updates.overviewCameraController = OverviewCameraController.instance;
        Updates.cameraShake = CameraShake.instance;

        Updates.puzzleeGoals = PuzzleGoals.instance;
    }

    void Update()
    {
        GameTime.DoUpdate();
        Updates.autoSaveLoad.DoUpdate();

        Updates.backgroundMusic.DoUpdate();
        Updates.machineDroppers.DoUpdate();

        // Camera transform stack
        Updates.overviewCameraController.DoUpdate();
        Updates.cameraShake.DoUpdate();

        // Rely on camera position
        Updates.linkConveyorButtons.DoUpdate();
        Updates.picker.DoUpdate();
        Updates.touchInput.DoUpdate();
    }

    void FixedUpdate()
    {
        GameTime.DoFixedUpdate();

        Updates.conveyors.DoFixedUpdate();
        Updates.machinePurchasers.DoFixedUpdate();
        Updates.machineSellers.DoFixedUpdate();
        Updates.machineAssemblers.DoFixedUpdate();
        Updates.machinePlacers.DoFixedUpdate();

        Updates.puzzleeGoals.DoFixedUpdate();
    }
}