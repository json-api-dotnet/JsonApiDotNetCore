namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery
    {
        public FilterQuery(AttrAttribute filteredAttribute, string propertyValue, FilterOperations filterOperation)
        {
            FilteredAttribute = filteredAttribute;
            PropertyValue = propertyValue;
            FilterOperation = filterOperation;
        }
        
        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
        public FilterOperations FilterOperation { get; set; }
    }
}