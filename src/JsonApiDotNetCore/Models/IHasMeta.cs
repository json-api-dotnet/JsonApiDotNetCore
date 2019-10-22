using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    public interface IHasMeta
    {
        Dictionary<string, object> GetMeta();
    }
}
