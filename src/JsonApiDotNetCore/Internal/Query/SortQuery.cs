namespace JsonApiDotNetCore.Internal.Query
{
    public class SortQuery
    {
        public SortQuery(SortDirection direction, AttrAttribute sortedAttribute)
        {
            Direction = direction;
            SortedAttribute = sortedAttribute;
        }
        public SortDirection Direction { get; set; }
        public AttrAttribute SortedAttribute { get; set; }
    }
}