using UnityEngine;

public static class MachinePlacerGen
{
    public static Bounds3Int MachinePlacementPosition(MachineInfo machineInfo, LandParcel landParcel)
    {
        // Iterate through all positions randomly
        Bounds3Int boundingBox = Bounds3Int.BoundingBox(landParcel.bounds);
        Vector3Int size = boundingBox.size;
        int[] xPositions = new int[size.x];
        int[] zPositions = new int[size.z];
        Mathx.PopulateRange(xPositions, boundingBox.min.x);
        Mathx.PopulateRange(zPositions, boundingBox.min.z);
        xPositions.Shuffle();
        zPositions.Shuffle();
        int y = boundingBox.min.y;

        for (int i = 0, len = xPositions.Length; i < len; ++i)
        {
            for (int j = 0, jlen = zPositions.Length; j < jlen; ++j)
            {
                Vector3Int position = new Vector3Int(xPositions[i], y, zPositions[j]);
                Bounds3Int bounds = Bounds3Int.Create(position, machineInfo.size);
                if (MachineSystem.instance.CanCreateMachine(machineInfo, bounds))
                {
                    return bounds;
                }
            }
        }

        // Unlikely
        return Bounds3Int.Create(new Vector3Int(0, 0, 0), machineInfo.size);
    }
}
