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
        List<LandParcel> overlap = landParcelHash.GetOverlap(bounds);
        bool valid = LandParcel.AllCanBuild(bounds, overlap);
        ListPool<LandParcel>.Release(overlap);
        return valid;
    }

    public bool CanBuild(Vector3Int position)
    {
        List<LandParcel> overlap = landParcelHash.GetOverlap(position);
        bool valid = LandParcel.AllCanBuild(overlap);
        ListPool<LandParcel>.Release(overlap);
        return valid;
    }

    void OnDrawGizmosSelected()
    {
        landParcelHash.Initialize();
        landParcelHash.DrawGizmos();
    }
}
