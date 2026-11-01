using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonApiDotNetCore.Resources;

internal static class ResourceChangeTrackerSnapshot
{
    // Compound and collection attributes are captured as JSON, so that changes to members that are not exposed as attributes (for example a last-update
    // timestamp inside a compound type) are detected too. This uses private options, independent of the application's serializer options: public
    // properties and fields only, no naming policy, no converters, nothing skipped (nulls and default values are written), recursion is cut off, and NaN and
    // Infinity are representable. Limitations: members hidden through [JsonIgnore] (and non-public members) cannot be compared this way. Sets are written
    // in enumeration order, so a set that enumerates in a different order than before can show up as a change.
    public static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        IncludeFields = true,
        WriteIndented = false
    };
}
