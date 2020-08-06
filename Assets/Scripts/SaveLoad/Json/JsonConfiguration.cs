using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;
using System.Collections.Generic;

public static class JsonConfiguration
{
    public static void Configure()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>
            {
                // From https://github.com/jilleJr/Newtonsoft.Json-for-Unity.Converters/blob/master/Doc/Compatability-table.md
                new Vector3Converter(),
                new Vector3IntConverter(),

                new Bounds3IntConverter()
            }
        };

        JsonConvert.DefaultSettings = () => settings;
    }
}
