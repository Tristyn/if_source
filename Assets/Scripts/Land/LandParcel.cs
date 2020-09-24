using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum LandParcelFlags
{
    None = 0,
    Restricted = 1,
    Valid = 2
}

[Serializable]
public sealed class LandParcel
{
    public SpacePlatform spacePlatform;
    public LandParcelFlags flags;
    public Bounds3Int[] bounds = Array.Empty<Bounds3Int>();

    public bool CanBuild()
    {
        return flags == LandParcelFlags.Valid;
    }

    public static bool AllCanBuild(List<LandParcel> landParcels)
    {
        bool valid = false;

        // Need to check that every point in the machine is within valid parcels
        for (int i = 0, len = landParcels.Count; i < len; ++i)
        {
            LandParcel landParcel = landParcels[i];
            if (landParcel.flags == LandParcelFlags.Restricted)
            {
                return false;
            }
            if (landParcel.flags == LandParcelFlags.Valid)
            {
                valid = true;
            }
        }

        return valid;
    }

    public static bool AllCanBuild(Bounds3Int bounds, List<LandParcel> overlappingLandParcels)
    {
        if (!AllCanBuild(overlappingLandParcels))
        {
            return false;
        }

        Vector3Int position = new Vector3Int(bounds.min.x, bounds.min.y, bounds.min.z);
        for (position.y = bounds.min.y; position.y <= bounds.max.y; ++position.y)
        {
            for (position.x = bounds.min.x; position.x <= bounds.max.x; ++position.x)
            {
                for (position.z = bounds.min.z; position.z <= bounds.max.z; ++position.z)
                {
                    bool positionContained = false;
                    for (int i = 0, len = overlappingLandParcels.Count; i < len; ++i)
                    {
                        LandParcel landParcel = overlappingLandParcels[i];
                        if (Bounds3Int.Contains(landParcel.bounds, position))
                        {
                            positionContained = true;
                        }
                    }
                    if (!positionContained)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}