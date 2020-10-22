using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SpacePlatform
{
    public SpacePlatformVisual visual;
    public LandParcel landParcel;

    public static HashSet<SpacePlatform> spacePlatforms = new HashSet<SpacePlatform>();

    [Serializable]
    public struct Saves
    {
        public Save[] spacePlatforms;
    }
    [Serializable]
    public struct Save
    {
        public Bounds3Int[] bounds;
        public Color color;
    }
    public Save save;

    public SpacePlatform()
    {
        save.color = Color.white;
    }

    public static void GetSave(out Saves save)
    {
        Save[] saves = new Save[spacePlatforms.Count];
        int i = 0;
        foreach (var spacePlatform in spacePlatforms)
        {
            saves[i] = spacePlatform.save;
            ++i;
        }
        save.spacePlatforms = saves;
    }

    public static void SetSave(in Saves save)
    {
        foreach (var spacePlatform in spacePlatforms)
        {
            spacePlatform.Delete();
        }
        spacePlatforms.Clear();

        Save[] saves = save.spacePlatforms ?? Array.Empty<Save>();
        for (int i = 0, len = saves.Length; i < len; ++i)
        {
            SpacePlatform spacePlatform = new SpacePlatform();
            spacePlatform.save = saves[i];
            spacePlatform.Initialize();
        }
    }

    public void Initialize()
    {
        save.bounds.ThrowOnNullOrEmpty();

        spacePlatforms.Add(this);

        landParcel = new LandParcel
        {
            spacePlatform = this,
            flags = LandParcelFlags.Valid,
            bounds = save.bounds
        };
        LandSystem.instance.AddLandParcel(landParcel);

        visual = new SpacePlatformVisual();
        visual.color = save.color;
        visual.Initialize(save.bounds);
    }

    public void Delete()
    {
        spacePlatforms.Remove(this);
        LandSystem.instance.RemoveLandParcel(landParcel);
        visual.Delete();
    }
}
