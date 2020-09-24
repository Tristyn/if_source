using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class SpatialHash
{
    public const int CELL_MASK = ~0b111;
    public const int CELL_SIZE = 0b1000;
}

[Serializable]
public struct SpatialHashEntry<T> where T : class
{
    public Bounds3Int bounds;
    public T value;

    public static bool operator ==(SpatialHashEntry<T> a, SpatialHashEntry<T> b)
    {
        return a.value == b.value && a.bounds == b.bounds;
    }

    public static bool operator !=(SpatialHashEntry<T> a, SpatialHashEntry<T> b)
    {
        return a.value != b.value || a.bounds != b.bounds;
    }

    public override bool Equals(object obj)
    {
        if (obj is SpatialHashEntry<T>)
        {
            var entry = (SpatialHashEntry<T>)obj;
            return this == entry;

        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = (hash * 7) + bounds.GetHashCode();
            hash = (hash * 7) + value.GetHashCode();
            return hash;
        }
    }
}

[Serializable]
public struct SpatialHash<T> where T : class
{
    // 3D spacial buckets where entries are an AABB Bounds3Int
    // A trade off was made to use a Vector3Int bucket id instead of an int hash
    // because calculating the hash requires setting the maximum dimensions.
    // Storing the bucket id as a Vector3Int uses 8 more bytes of memory per bucket.



    public Dictionary<Vector3Int, List<SpatialHashEntry<T>>> buckets;

    public bool initialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buckets != null;
    }

    public void Initialize()
    {
        if (!initialized)
        {
            buckets = new Dictionary<Vector3Int, List<SpatialHashEntry<T>>>();
        }
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

    public void Add(T value, Bounds3Int[] bounds)
    {
        Assert.IsNotNull(value);
        Assert.IsNotNull(bounds);
        for (int i = 0, len = bounds.Length; i < len; ++i)
        {
            Add(value, in bounds[i]);
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

    public void Remove(T value, Bounds3Int[] bounds)
    {
        Assert.IsNotNull(value);
        Assert.IsNotNull(bounds);
        for (int i = 0, len = bounds.Length; i < len; ++i)
        {
            Remove(value, in bounds[i]);
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

    /// <summary>
    /// Gets all entrys that overlap with bounds.
    /// The list can be recycled with ListPool<SpatialHashEntry<T>>.Release()
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public List<T> GetOverlap(Bounds3Int bounds)
    {
        List<T> results = ListPool<T>.Get();
        Vector3Int min = GetBucketId(bounds.min);
        Vector3Int max = GetBucketId(bounds.max);
        for (int y = min.y; y <= max.y; y += SpatialHash.CELL_SIZE)
        {
            for (int x = min.x; x <= max.x; x += SpatialHash.CELL_SIZE)
            {
                for (int z = min.z; z <= max.z; z += SpatialHash.CELL_SIZE)
                {
                    Vector3Int bucketId = new Vector3Int(x, y, z);
                    GetOverlap(bounds, bucketId, results);
                }
            }
        }
        return results;
    }

    /// <summary>
    /// Gets all entrys that contain position.
    /// The list can be recycled with ListPool<SpatialHashEntry<T>>.Release()
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public List<T> GetOverlap(Vector3Int position)
    {
        List<T> results = ListPool<T>.Get();
        Vector3Int bucketId = GetBucketId(position);
        GetOverlap(position, bucketId, results);
        return results;
    }

    public T GetFirst(Vector3Int position)
    {
        Vector3Int bucketId = GetBucketId(position);
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
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
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            bucket.Add(new SpatialHashEntry<T>
            {
                bounds = bounds,
                value = value
            });
        }
        else
        {
            bucket = new List<SpatialHashEntry<T>>(4);
            bucket.Add(new SpatialHashEntry<T>
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
        List<SpatialHashEntry<T>> bucket = buckets[bucketId];
        for (int i = bucket.Count - 1; i >= 0; --i)
        {
            SpatialHashEntry<T> entry = bucket[i];
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
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
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
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
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
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
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
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
                if (entry.bounds.Contains(position))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOverlap(Bounds3Int bounds, Vector3Int bucketId, List<T> results)
    {
        int numResults = 0;
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
                if (bounds.Overlaps(entry.bounds) && !results.Contains(entry.value))
                {
                    results.Add(entry.value);
                    ++numResults;
                }
            }
        }
        return numResults;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOverlap(Vector3Int position, Vector3Int bucketId, List<T> results)
    {
        int numResults = 0;
        if (buckets.TryGetValue(bucketId, out List<SpatialHashEntry<T>> bucket))
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                SpatialHashEntry<T> entry = bucket[i];
                if (entry.bounds.Contains(position) && !results.Contains(entry.value))
                {
                    results.Add(entry.value);
                    ++numResults;
                }
            }
        }
        return numResults;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3Int GetBucketId(Vector3Int position)
    {
        return new Vector3Int(
            position.x & SpatialHash.CELL_MASK,
            position.y & SpatialHash.CELL_MASK,
            position.z & SpatialHash.CELL_MASK);
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (List<SpatialHashEntry<T>> bucket in buckets.Values)
        {
            for (int i = 0, len = bucket.Count; i < len; ++i)
            {
                Bounds3Int bounds = bucket[i].bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}
