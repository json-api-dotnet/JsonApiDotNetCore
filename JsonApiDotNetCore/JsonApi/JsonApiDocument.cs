using System.Collections.Generic;

namespace JsonApiDotNetCore.JsonApi
{
    public class JsonApiDocument
    {
        public Dictionary<string, string> Links { get; set; }
        public object Data { get; set; }
    }
}
