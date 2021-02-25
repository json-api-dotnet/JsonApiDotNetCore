using System.Collections.Generic;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <summary>
    /// Builds the top-level meta object.
    /// </summary>
    [PublicAPI]
    public interface IMetaBuilder
    {
        /// <summary>
        /// Merges the specified dictionary with existing key/value pairs. In the event of a key collision, the value from the specified dictionary will
        /// overwrite the existing one.
        /// </summary>
        void Add(IReadOnlyDictionary<string, object> values);

        /// <summary>
        /// Builds the top-level meta data object.
        /// </summary>
        IDictionary<string, object> Build();
    }
}
