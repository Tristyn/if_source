using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class AddonGen
{
    public static Bounds3Int[] Addon()
    {
        Bounds3Int[] addon = GenerateAddon(minArea: 200);
        Bounds3Int[] placedAddon = PlaceAddon(addon, LandSystem.instance.landParcelHash);
        return placedAddon;
    }

    public static Bounds3Int[] GenerateAddon(int minArea)
    {
        using (ListPool<Bounds3Int>.Get(out List<Bounds3Int> addons))
        {
            Vector3Int sizeMin = new Vector3Int(7, 1, 7);
            Vector3Int sizeMax = new Vector3Int(10, 1, 10);

            Bounds3Int addonBounds;
            using (ListPool<Bounds3Int>.Get(out List<Bounds3Int> stub))
            {
                stub.Add(new Bounds3Int(0, 0, 0, 1, 1, 1));
                addonBounds = IterateAddonBounds(stub, sizeMin, sizeMax, addons);
            }

            int area = addonBounds.area;
            addons.Add(addonBounds);

            Vector3Int size2Min = new Vector3Int(4, 1, 4);
            Vector3Int size2Max = new Vector3Int(7, 1, 7);

            while (area < minArea)
            {
                Bounds3Int subAddon = IterateAddonBounds(addons, size2Min, size2Max, addons);
                area += subAddon.area;
                addons.Add(subAddon);
            }

            return addons.ToArray();
        }
    }

    static Bounds3Int IterateAddonBounds(List<Bounds3Int> adjacentTo, Vector3Int sizeMin, Vector3Int sizeMax, List<Bounds3Int> occupied)
    {
        while (true)
        {
            for (int k = 0, lenk = adjacentTo.Count; k < lenk; ++k)
            {
                using (ListPool<Directions>.Get(out List<Directions> sides))
                {
                    // Try sides randomly
                    sides.AddArray(EnumUtil<Directions>.values);
                    sides.Shuffle();
                    for (int l = 0, lenl = sides.Count; l < lenl; ++l)
                    {
                        Directions side = sides[l];

                        int i = 0;
                        int maxIterations = 10;
                        bool overlaps;
                        Bounds3Int addonBounds;
                        do
                        {
                            addonBounds = GetAddonBounds(adjacentTo[k], side, sizeMin, sizeMax);

                            overlaps = addonBounds.Overlaps(occupied);
                            if (overlaps)
                            {
                                break;
                            }
                            ++i;
                        } while (overlaps && i < maxIterations);
                        if (!overlaps)
                        {
                            return addonBounds;
                        }
                    }
                }
            }
        }
    }

    static Bounds3Int GetAddonBounds(Bounds3Int adjacentTo, Directions side, Vector3Int sizeMin, Vector3Int sizeMax)
    {
        Vector3Int min = adjacentTo.GetMin(side);
        Vector3Int max = adjacentTo.GetMax(side);

        Vector3Int randomAdjacentEdge = Vector3.Lerp(min, max, UnityEngine.Random.value).RoundDown();
        Vector3Int addonEdgeCenter = randomAdjacentEdge + side.ToOffsetInt();

        Vector3Int size = Mathx.RandomRange(sizeMin, sizeMax);

        Vector3Int addonLeftEdge = addonEdgeCenter + side.Left().ToOffsetInt((int)(size.x * 0.5f));
        Vector3Int addonRightEdge = addonEdgeCenter + side.Right().ToOffsetInt((int)(size.x * 0.5f));
        Vector3Int addonFarRightEdge = addonRightEdge + side.ToOffsetInt(size.z);

        Bounds3Int addonBounds = Bounds3Int.FromPoints(addonLeftEdge, addonFarRightEdge);
        return addonBounds;
    }

    static Bounds3Int[] PlaceAddon(Bounds3Int[] addon, SpatialHash<LandParcel> occupied)
    {
        float angle = UnityEngine.Random.value * Mathf.PI * 2;
        Bounds3Int[] placed = PlaceAddonRadially(angle, addon, occupied, out Vector2Int position);
        ShiftTowardsZero(addon, placed, occupied, ref position);
        return placed;
    }

    static Bounds3Int[] PlaceAddonRadially(float radianAngle, Bounds3Int[] addon, SpatialHash<LandParcel> occupied, out Vector2Int position)
    {
        Vector2 direction = Mathx.RadianToVector2(radianAngle);
        PixelTraversal tracer = new PixelTraversal(new Vector2(), direction);

        Bounds3Int[] placed = new Bounds3Int[addon.Length];
        bool overlaps;
        do
        {
            tracer.Step();
            overlaps = false;

            for (int i = 0, len = addon.Length; i < len; ++i)
            {
                Bounds3Int translated = addon[i].Translate(tracer.position.ToVector3XZ());
                if (occupied.Overlaps(translated))
                {
                    overlaps = true;
                    break;
                }
                placed[i] = translated;
            }

        } while (overlaps);

        position = tracer.position;
        return placed;
    }

    static void ShiftTowardsZero(Bounds3Int[] addon, Bounds3Int[] placed, SpatialHash<LandParcel> occupied, ref Vector2Int position)
    {
        bool movedX;
        bool movedY;
        do
        {
            movedX = ShiftTowardsZero(addon, placed, occupied, positionIndex: 0, ref position);
            movedY = ShiftTowardsZero(addon, placed, occupied, positionIndex: 1, ref position);
        } while (movedX || movedY);
    }

    static bool ShiftTowardsZero(Bounds3Int[] addon, Bounds3Int[] placed, SpatialHash<LandParcel> occupied, int positionIndex, ref Vector2Int placedPosition)
    {
        Vector2Int position = placedPosition;
        if (position[positionIndex] == 0)
        {
            return false;
        }

        int step = position[positionIndex] > 0 ? -1 : 1;
        bool overlaps;
        const int maxIterations = 20;
        int iter = 0;
        do
        {
            overlaps = false;
            position[positionIndex] += step;
            if (position[positionIndex] == step)
            {
                break;
            }

            if (iter >= maxIterations)
            {
                // Failsafe in case we miss the geometry and shift into oblivion
                for (int i = 0, len = addon.Length; i < len; ++i)
                {
                    placed[i] = addon[i].Translate(placedPosition.ToVector3XZ());
                }
                return false;
            }
            ++iter;


            for (int i = 0, len = addon.Length; i < len; ++i)
            {
                Bounds3Int translated = addon[i].Translate(position.ToVector3XZ());
                if (occupied.Overlaps(translated))
                {
                    overlaps = true;
                }
                placed[i] = translated;
            }

        } while (!overlaps);

        position[positionIndex] -= step;
        for (int i = 0, len = addon.Length; i < len; ++i)
        {
            placed[i] = addon[i].Translate(position.ToVector3XZ());
            //Shouldn't overlap
        }

        bool moved = position[positionIndex] != placedPosition[positionIndex];
        placedPosition = position;
        return moved;
    }
}
