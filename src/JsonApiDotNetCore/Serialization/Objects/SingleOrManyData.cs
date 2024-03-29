using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// Represents the value of the "data" element, which is either null, a single object or an array of objects. Add
/// <see cref="SingleOrManyDataConverterFactory" /> to <see cref="JsonSerializerOptions.Converters" /> to properly roundtrip.
/// </summary>
/// <typeparam name="T">
/// The type of elements being wrapped, typically <see cref="ResourceIdentifierObject" /> or <see cref="ResourceObject" />.
/// </typeparam>
[PublicAPI]
public readonly struct SingleOrManyData<T>
    // The "new()" constraint exists for parity with SingleOrManyDataConverterFactory, which creates empty instances
    // to ensure ManyValue never contains null items.
    where T : ResourceIdentifierObject, new()
{
    public object? Value => ManyValue != null ? ManyValue : SingleValue;

    [JsonIgnore]
    public bool IsAssigned { get; }

    [JsonIgnore]
    public T? SingleValue { get; }

    [JsonIgnore]
    public IList<T>? ManyValue { get; }

    public SingleOrManyData(object? value)
    {
        IsAssigned = true;

        if (value is IEnumerable<T> manyData)
        {
            ManyValue = manyData.ToList();
            SingleValue = null;
        }
        else
        {
            ManyValue = null;
            SingleValue = (T?)value;
        }
    }
}
