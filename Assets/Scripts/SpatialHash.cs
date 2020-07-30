using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public static class SpatialHash
{
    public const int CELL_MASK = ~0b1111;
    public const int CELL_SIZE = 0b10000;
    public const int CELL_BITS = 4; // num bits used by the cell mask
}

[Serializable]
public struct SpatialHash<T> where T : class
{
    // 3D spacial buckets where entries are an AABB Bounds3Int
    // A trade off was made to use a Vector3Int bucket id instead of an int hash
    // because calculating the hash requires setting the maximum dimensions.
    // Storing the bucket id as a Vector3Int uses 8 more bytes of memory per bucket.

    [Serializable]
    public struct SpatialHashEntry
    {
        public Bounds3Int bounds;
        public T value;
    }

    public Dictionary<Vector3Int, List<SpatialHashEntry>> buckets;

    public bool initialized => buckets != null;

    public void Initialize()
    {
        buckets = new Dictionary<Vector3Int, List<SpatialHashEntry>>();
    }

    public void Clear()
    {
        buckets.Clear();
    }

    public void Add(T value, in Bounds3Int bounds)
    {
        Assert.IsNotNull(value);
        Vector3Int min = GetBucketId(bounds.min);
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    Add(value, in bounds, bucketId);
                }
            }
        }
    }

    public void Remove(T value, in Bounds3Int bounds)
    {
        Vector3Int min = GetBucketId(bounds.min);
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    Remove(value, in bounds, bucketId);
                }
            }
        }
    }

    public bool Contains(T value, Bounds3Int bounds)
    {
        Vector3Int min = GetBucketId(bounds.min);
        bool contains = Contains(value, bounds, min);
#if UNITY_ASSERTIONS
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    Assert.IsTrue(contains == Contains(value, bounds, bucketId));
                }
            }
        }
#endif
        return contains;
    }

    // Do any other objects overlap this one
    public bool Overlaps(Vector3Int position)
    {
        Vector3Int bucketId = GetBucketId(position);
        return Overlaps(position, bucketId);
    }

    // Do any other objects overlap this one
    public bool Overlaps(T value, Bounds3Int bounds)
    {
        Vector3Int min = GetBucketId(bounds.min);
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    if (Overlaps(value, bounds, bucketId))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Would bounds overlap with any entries
    public bool Overlaps(Bounds3Int bounds)
    {
        Vector3Int min = GetBucketId(bounds.min);
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    if (Overlaps(bounds, bucketId))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public T GetSingle(Vector3Int position)
    {
        Vector3Int bucketId = GetBucketId(position);
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                var entry = bucket[i];
                if (entry.bounds.Contains(position))
                {
                    return entry.value;
                }
            }
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Add(T value, in Bounds3Int bounds, Vector3Int bucketId)
    {
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            bucket.Add(new SpatialHashEntry
            {
                bounds = bounds,
                value = value
            });
        }
        else
        {
            bucket = new List<SpatialHashEntry>(4);
            bucket.Add(new SpatialHashEntry
            {
                bounds = bounds,
                value = value
            });
            buckets.Add(bucketId, bucket);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Remove(T value, in Bounds3Int bounds, Vector3Int bucketId)
    {
        List<SpatialHashEntry> bucket = buckets[bucketId];
        for (int i = bucket.Count - 1; i >= 0; --i)
        {
            SpatialHashEntry entry = bucket[i];
            if (entry.value == value)
            {
                Assert.IsTrue(bounds == entry.bounds);
                bucket.RemoveAtSwapBack(i);
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Contains(T value, Bounds3Int bounds, Vector3Int bucketId)
    {
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry entry = bucket[i];
                if (entry.value == value)
                {
                    Assert.IsTrue(bounds == entry.bounds);
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Overlaps(T value, Bounds3Int bounds, Vector3Int bucketId)
    {
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry entry = bucket[i];
                if (entry.value != value)
                {
                    if (bounds.Overlaps(entry.bounds))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Overlaps(Bounds3Int bounds, Vector3Int bucketId)
    {
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry entry = bucket[i];
                if (bounds.Overlaps(entry.bounds))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Overlaps(Vector3Int position, Vector3Int bucketId)
    {
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry entry = bucket[i];
                if (entry.bounds.Contains(position))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3Int GetBucketId(Vector3Int position)
    {
        return new Vector3Int(
            position.x & SpatialHash.CELL_MASK,
            position.y & SpatialHash.CELL_MASK,
            position.z & SpatialHash.CELL_MASK);
    }
}
