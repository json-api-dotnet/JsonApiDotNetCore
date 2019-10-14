using System;

namespace JsonApiDotNetCore.Controllers
{
    public class DisableQueryAttribute : Attribute
    { 
        public DisableQueryAttribute(QueryParams queryParams)
        {
            QueryParams = queryParams.ToString("G").ToLower();
        }

        public DisableQueryAttribute(string customQueryParams)
        {
            QueryParams = customQueryParams.ToLower();
        }

        public string QueryParams { get; }
    }
}