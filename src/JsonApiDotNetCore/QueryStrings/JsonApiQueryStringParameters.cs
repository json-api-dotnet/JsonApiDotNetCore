using System;
using JsonApiDotNetCore.Controllers.Annotations;

namespace JsonApiDotNetCore.QueryStrings
{
    /// <summary>
    /// Lists query string parameters used by <see cref="DisableQueryStringAttribute" />.
    /// </summary>
    [Flags]
    public enum JsonApiQueryStringParameters
    {
        None = 0,
        Filter = 1,
        Sort = 2,
        Include = 4,
        Page = 8,
        Fields = 16,
        All = Filter | Sort | Include | Page | Fields
    }
}
