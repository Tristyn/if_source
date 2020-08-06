using Newtonsoft.Json.UnityConverters;
using UnityEngine;
using UnityEngine.Scripting;

public sealed class Bounds3IntConverter : PartialVector3IntConverter<Bounds3Int>
{
    private static readonly string[] _memberNames = { "min", "max" };

    public Bounds3IntConverter() : base(_memberNames)
    {
    }

    /// <summary>
    /// Prevent the properties from being stripped.
    /// </summary>
    [Preserve]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members",
        Justification = "Ensures the properties are preserved, instead of adding a link.xml file.")]
    private static void PreserveProperties()
    {
        var dummy = new Bounds3Int();

        _ = dummy.min;
        _ = dummy.max;
    }

    protected override Bounds3Int CreateInstanceFromValues(ValuesArray<Vector3Int> values)
    {
        return new Bounds3Int(values[0], values[1]);
    }

    protected override Vector3Int[] ReadInstanceValues(Bounds3Int instance)
    {
        return new[] { instance.min, instance.max };
    }
}

