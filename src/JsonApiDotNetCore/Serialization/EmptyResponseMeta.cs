using System.Collections.Generic;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public sealed class EmptyResponseMeta : IResponseMeta
    {
        /// <inheritdoc />
        public IReadOnlyDictionary<string, object> GetMeta()
        {
            return null;
        }
    }
}
