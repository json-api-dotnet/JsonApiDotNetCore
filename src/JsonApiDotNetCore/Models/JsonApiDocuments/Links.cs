using System;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    [Flags]
    public enum Links
    {
        Self = 1 << 0,
        Related = 1 << 1,
        Paging = 1 << 2,
        NotConfigured = 1 << 3,
        None = 1 << 4,
        All = Self | Related | Paging
    }
}
