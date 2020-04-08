using System;

namespace JsonApiDotNetCore.Controllers
{
    [Flags]
    public enum StandardQueryStringParameters
    {
        None = 0,
        Filter = 1,
        Sort = 2,
        Include = 4,
        Page = 8,
        Fields = 16,
        // TODO: Rename to single-word to prevent violating casing conventions.
        OmitNull = 32,
        // TODO: Rename to single-word to prevent violating casing conventions.
        OmitDefault = 64,
        All = Filter | Sort | Include | Page | Fields | OmitNull | OmitDefault
    }
}
