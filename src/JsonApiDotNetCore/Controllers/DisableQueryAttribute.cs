using System;

namespace JsonApiDotNetCore.Controllers
{
    public class DisableQueryAttribute : Attribute
    { 
        public DisableQueryAttribute(QueryParams queryParams)
        {
            QueryParams = queryParams;
        }

        public QueryParams QueryParams { get; set; }
    }
}