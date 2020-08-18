using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    public interface IHasMeta
    {
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
