using UnityEngine;

public class SpacePlatformVisual
{
    public Color color = Color.white;
    public Floor[] floors;

    public void Initialize(Bounds3Int[] bounds)
    {
        Floor[] floors = new Floor[bounds.Length];
        this.floors = floors;
        for (int i = 0, len = bounds.Length; i < len; ++i)
        {
            Floor floor = ObjectPooler.instance.Get<Floor>();
            floors[i] = floor;

            floor.color = color;
            floor.Initialize(bounds[i]);
        }
    }

    public void Delete()
    {
        for (int i = 0, len = floors.Length; i < len; ++i)
        {
            if (floors[i])
            {
                ObjectPooler.instance.Recycle(floors[i]);
            }
        }
    }
}
