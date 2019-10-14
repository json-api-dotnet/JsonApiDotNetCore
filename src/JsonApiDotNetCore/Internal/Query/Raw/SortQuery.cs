namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Internal representation of the articles?sort[field] query from the URL.
    /// </summary>
    public class SortQuery : BaseQuery
    {
        public SortQuery(string target, SortDirection direction)
            : base(target)
        {
            Direction = direction;
        }

        /// <summary>
        /// Direction the sort should be applied
        /// </summary>
        public SortDirection Direction { get; set; }
    }


}
