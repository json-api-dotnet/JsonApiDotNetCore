using System;
using JsonApiDotNetCore.Controllers.Annotations;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Lists query string parameters used by <see cref="DisableQueryStringAttribute" />.
    /// </summary>
    [Flags]
    public enum StandardQueryStringParameters
    {
        None = 0,
        Filter = 1,
        Sort = 2,
        Include = 4,
        Page = 8,
        Fields = 16,
        Nulls = 32,
        Defaults = 64,
        All = Filter | Sort | Include | Page | Fields | Nulls | Defaults
    }
}
