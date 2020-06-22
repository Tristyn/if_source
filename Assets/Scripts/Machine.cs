using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Machine : MonoBehaviour
{
    public Bounds3Int bounds;
    public List<Conveyor> conveyors;
    public ItemInfo itemInfo;

    void Awake()
    {
        MachineSystem.instance.machines.Add(this);
    }

    private void OnDestroy()
    {
        if (MachineSystem.instance)
        {
            MachineSystem.instance.machines.Remove(this);
        }
    }

    public void AddConveyor(Conveyor conveyor)
    {
        Assert.IsFalse(conveyors.Contains(conveyor));
        Assert.IsTrue(bounds.Contains(conveyor.position));
        Assert.IsTrue(bounds.Perimeter(conveyor.position));
        Assert.IsNull(GetConveyor(conveyor.position));
        conveyors.Add(conveyor);
        conveyor.machine = this;
    }

    public void RemoveConveyor(Conveyor conveyor)
    {
        Assert.IsTrue(conveyors.Contains(conveyor));
        Assert.IsTrue(bounds.Contains(conveyor.position));
        Assert.IsTrue(bounds.Perimeter(conveyor.position));
        Assert.IsNotNull(GetConveyor(conveyor.position));
        conveyors.Remove(conveyor);
        conveyor.machine = null;
    }

    public Conveyor GetConveyor(Vector3Int position)
    {
        for (int i = 0, len = conveyors.Count; i < len; i++)
        {
            Conveyor conveyor = conveyors[i];
            if (conveyor.position == position)
            {
                return conveyor;
            }
        }
        return null;
    }

    public void Delete()
    {
        for(int i = 0, len = conveyors.Count; i < len; i++)
        {
            Conveyor conveyor = conveyors[i];
            RemoveConveyor(conveyor);
            conveyor.Recycle();
        }
        
        Destroy(gameObject);
    }
}
