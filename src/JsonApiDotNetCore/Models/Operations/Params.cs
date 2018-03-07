using System.Collections.Generic;

namespace JsonApiDotNetCore.Models.Operations
{
    public class Params
    {
        public List<string> Include { get; set; }
        public List<string> Sort { get; set; }
        public Dictionary<string, object> Filter { get; set; }
        public string Page { get; set; }
        public Dictionary<string, object> Fields { get; set; }
    }
}
