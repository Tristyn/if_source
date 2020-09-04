using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class LandSystem : Singleton<LandSystem>
{
    public struct Save
    {
        public LandParcel[] landParcels;
    }

    public LandParcel[] landParcelsList;
    [HideInInspector]
    public SpatialHash<LandParcel> landParcels;

    protected override void Awake()
    {
        base.Awake();
        landParcels.Initialize();
        for (int i = 0, len = landParcelsList.Length; i < len; ++i)
        {
            landParcels.Add(landParcelsList[i], landParcelsList[i].bounds);
        }
    }

    public void GetSave(out Save save)
    {
        save.landParcels = landParcelsList;
    }

    public void SetSave(in Save save)
    {
        landParcelsList = save.landParcels ?? Array.Empty<LandParcel>();

        landParcels.Clear();
        if (save.landParcels != null)
        {
            for (int i = 0, len = save.landParcels.Length; i < len; ++i)
            {
                LandParcel landParcel = save.landParcels[i];
                landParcels.Add(landParcel, landParcel.bounds);
            }
        }
    }

    public void AddLandParcel(LandParcel land)
    {
        if (landParcelsList.Contains(land))
        {
            Debug.LogWarning("Land parcel added twice", this);
            return;
        }
        landParcelsList = landParcelsList.Append(land);
        landParcels.Add(land, land.bounds);
    }

    public void RemoveLandParcel(LandParcel land)
    {
        landParcelsList = landParcelsList.Remove(land);
        landParcels.Remove(land, land.bounds);
    }

    public bool CanBuild(Bounds3Int bounds)
    {
        // Not the fastest algorithm but machines are pretty small so it's not too slow
        Vector3Int position = new Vector3Int(bounds.min.x, bounds.min.y, bounds.min.z);
        for (position.y = bounds.min.y; position.y <= bounds.max.y; ++position.y)
        {
            for (position.x = bounds.min.x; position.x <= bounds.max.x; ++position.x)
            {
                for (position.z = bounds.min.z; position.z <= bounds.max.z; ++position.z)
                {
                    if (!CanBuild(position))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public bool CanBuild(Vector3Int position)
    {
        List<LandParcel> results = landParcels.GetOverlap(position);
        bool valid = false;

        // Need to check that every point in the machine is within valid parcels
        for (int i = 0, len = results.Count; i < len; ++i)
        {
            LandParcel landParcel = results[i];
            if (landParcel.Flags == LandParcelFlags.Restricted)
            {
                return false;
            }
            if (landParcel.Flags == LandParcelFlags.Valid)
            {
                valid = true;
            }
        }

        ListPool<LandParcel>.Release(results);
        return valid;
    }

    void OnDrawGizmosSelected()
    {
        landParcels.Initialize();
        Gizmos.color = Color.white;
        foreach (List<SpatialHashEntry<LandParcel>> bucket in landParcels.buckets.Values)
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                Bounds3Int bounds = bucket[i].bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}
