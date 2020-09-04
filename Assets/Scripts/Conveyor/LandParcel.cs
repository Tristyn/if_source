
using System;

[Flags]
public enum LandParcelFlags
{
    None = 0,
    Restricted = 1,
    Valid = 2
}

[Serializable]
public class LandParcel
{
    public LandParcelFlags Flags;
    public Bounds3Int[] bounds = Array.Empty<Bounds3Int>();
}