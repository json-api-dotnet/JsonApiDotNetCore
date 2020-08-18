using System.Collections.Generic;

namespace JsonApiDotNetCore.Resources
{
    public interface IHasMeta
    {
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
