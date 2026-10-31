using System.Text.Json.Serialization;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Represents the value of a compound attribute in a response, after applying sparse fieldsets and view restrictions. Its member names are written to JSON
/// verbatim, instead of being rewritten by the dictionary key policy.
/// </summary>
[JsonConverter(typeof(CompoundObjectValueConverter))]
internal sealed class CompoundObjectValue : Dictionary<string, object?>;
