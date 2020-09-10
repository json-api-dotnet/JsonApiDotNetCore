using System.Collections.Generic;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// When implemented by a class, indicates it provides json:api meta key/value pairs.
    /// </summary>
    public interface IHasMeta
    {
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
