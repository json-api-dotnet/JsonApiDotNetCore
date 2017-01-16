namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery
    {
        public FilterQuery(AttrAttribute filteredAttribute, string propertyValue)
        {
            FilteredAttribute = filteredAttribute;
            PropertyValue = propertyValue;
        }
        
        public AttrAttribute FilteredAttribute { get; set; }
        public string PropertyValue { get; set; }
    }
}