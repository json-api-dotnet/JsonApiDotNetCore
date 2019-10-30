namespace JsonApiDotNetCore.Controllers
{
    public enum QueryParams
    { 
        Filters = 1 << 0,
        Sort = 1 << 1,
        Include = 1 << 2,
        Page = 1 << 3,
        Fields = 1 << 4,
        All = ~(-1 << 5),
        None = 1 << 6,
    }
}