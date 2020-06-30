using UnityEngine;


[RequireComponent(typeof(MachinePlacer))]
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

    void Update()
    {
        if (nextPlaceTime >= Time.time)
        {
            nextPlaceTime += machineInfo.placeInterval;
            if (machinePlacer.PlaceItem())
            {

            }
        }
    }
}