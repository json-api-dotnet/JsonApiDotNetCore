// ReSharper disable InconsistentNaming
namespace JsonApiDotNetCore.Internal.Query
{
    public enum FilterOperation
    {
        eq = 0,
        lt = 1,
        gt = 2,
        le = 3,
        ge = 4,
        like = 5,
        ne = 6,
        @in = 7, // prefix with @ to use keyword
        nin = 8,
        isnull = 9,
        isnotnull = 10,
        notlike = 11,
        sw = 12, //start with
        notsw = 13, //doesn't start with
        ew = 14, // end with,
        notew = 15 // doesn't end with
    }
}
