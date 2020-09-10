using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class LandSystem : Singleton<LandSystem>
{
    public HashSet<LandParcel> landParcelSet = new HashSet<LandParcel>();
    public SpatialHash<LandParcel> landParcelHash;

    protected override void Awake()
    {
        base.Awake();
        landParcelHash.Initialize();
        foreach (LandParcel landParcel in landParcelSet)
        {
            landParcelHash.Add(landParcel, landParcel.bounds);
        }
    }

    public void AddLandParcel(LandParcel land)
    {
        if (landParcelSet.Contains(land))
        {
            Debug.LogWarning("Land parcel added twice", this);
            return;
        }
        landParcelSet.Add(land);
        landParcelHash.Add(land, land.bounds);
    }

    public void RemoveLandParcel(LandParcel land)
    {
        landParcelSet.Remove(land);
        landParcelHash.Remove(land, land.bounds);
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
        List<LandParcel> results = landParcelHash.GetOverlap(position);
        bool valid = false;

        // Need to check that every point in the machine is within valid parcels
        for (int i = 0, len = results.Count; i < len; ++i)
        {
            LandParcel landParcel = results[i];
            if (landParcel.flags == LandParcelFlags.Restricted)
            {
                return false;
            }
            if (landParcel.flags == LandParcelFlags.Valid)
            {
                valid = true;
            }
        }

        ListPool<LandParcel>.Release(results);
        return valid;
    }

    void OnDrawGizmosSelected()
    {
        landParcelHash.Initialize();
        Gizmos.color = Color.white;
        foreach (List<SpatialHashEntry<LandParcel>> bucket in landParcelHash.buckets.Values)
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                Bounds3Int bounds = bucket[i].bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Bounds3Int[] spacePlatformBounds = AddonGen.Addon();
            SpacePlatform spacePlatform = new SpacePlatform();
            spacePlatform.save.bounds = spacePlatformBounds;
            spacePlatform.save.color = new Color(Random.value, Random.value, Random.value);
            spacePlatform.Initialize();
            OverviewCameraController.instance.MoveTo(spacePlatform.visual.floors[0].transform.position);
        }
    }
}
