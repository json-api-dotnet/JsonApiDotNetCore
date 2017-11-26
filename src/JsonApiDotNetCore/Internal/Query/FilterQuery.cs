using System;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery
    {
        public FilterQuery(string attribute, string value, string operation)
        {
            Attribute = attribute;
            Key = attribute.ToProperCase();
            Value = value;
            Operation = operation;
        }
        
        [Obsolete("Key has been replaced by '" + nameof(Attribute) + "'. Members should be located by their public name, not by coercing the provided value to the internal name.")]
        public string Key { get; set; }
        public string Attribute { get; }
        public string Value { get; set; }
        public string Operation { get; set; }
        public bool IsAttributeOfRelationship => Attribute.Contains(".");
    }
}
