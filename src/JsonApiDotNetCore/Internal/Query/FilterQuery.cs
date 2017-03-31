namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery
    {
        public FilterQuery(string key, string value, string operation)
        {
            Key = key;
            Value = value;
            Operation = operation;
        }
        
        public string Key { get; set; }
        public string Value { get; set; }
        public string Operation { get; set; }
    }
}