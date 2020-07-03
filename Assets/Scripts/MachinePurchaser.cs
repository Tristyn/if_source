using UnityEngine;


public class MachinePurchaser : MonoBehaviour
{
    public Machine machine;

    MachineInfo machineInfo;
    MachinePlacer machinePlacer;

    float nextPlaceTime = -1f;

    public void Initialize()
    {
        machinePlacer = GetComponent<MachinePlacer>();
        machineInfo = machine.machineInfo;
    }

    void FixedUpdate()
    {
        if (nextPlaceTime <= Time.fixedTime)
        {
            nextPlaceTime += machineInfo.placeInterval;
            if (machinePlacer.PlaceItem())
            {

            }
        }
    }
}