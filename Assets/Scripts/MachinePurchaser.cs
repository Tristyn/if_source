using UnityEngine;


[RequireComponent(typeof(MachinePlacer))]
public class MachinePurchaser : MonoBehaviour
{
    ItemInfo itemInfo;
    public MachinePlacer machinePlacer;

    float nextPlaceTime = -1f;

    private void Awake()
    {
        machinePlacer = GetComponent<MachinePlacer>();
        Machine machine = GetComponent<Machine>();
        if (machine)
        {
            itemInfo = machine.itemInfo;
        }
    }

    void Update()
    {
        if (nextPlaceTime >= Time.time)
        {
            nextPlaceTime += itemInfo.placeInterval;
            if (machinePlacer.PlaceItem())
            {

            }
        }
    }
}