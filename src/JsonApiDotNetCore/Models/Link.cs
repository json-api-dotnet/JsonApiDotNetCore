using System;

namespace JsonApiDotNetCore.Models
{
    [Flags]
    public enum Link
    {
        Self = 1 << 0,
        Paging = 1 << 1,
        Related = 1 << 2,
        All = ~(-1 << 3),
        None = 1 << 4,
    }
}
