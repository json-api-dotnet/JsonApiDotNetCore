#nullable disable

using System.Collections.Generic;

namespace JsonApiDotNetCore.Serialization.Response
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
