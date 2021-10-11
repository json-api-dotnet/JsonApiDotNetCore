using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// Represents the value of the "data" element, which is either null, a single object or an array of objects. Add
    /// <see cref="SingleOrManyDataConverterFactory" /> to <see cref="JsonSerializerOptions.Converters" /> to properly roundtrip.
    /// </summary>
    [PublicAPI]
    public readonly struct SingleOrManyData<T>
        // The "new()" constraint exists for parity with SingleOrManyDataConverterFactory, which creates empty instances
        // to ensure ManyValue never contains null items.
        where T : class, IResourceIdentity, new()
    {
        // ReSharper disable once MergeConditionalExpression
        // Justification: ReSharper reporting this is a bug, which is fixed in v2021.2.1. This condition cannot be merged.
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
}
