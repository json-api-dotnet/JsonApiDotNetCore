namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// represents what FilterQuery and SortQuery have in common: a target.
    /// (sort=TARGET, or filter[TARGET]=123).
    /// </summary>
    public abstract class BaseQuery
    {
        protected BaseQuery(string target)
        {
            Target = target;
            var properties = target.Split(QueryConstants.DOT);
            if (properties.Length > 1)
            {
                Relationship = properties[0];
                Attribute = properties[1];
            }
            else
                Attribute = properties[0];
        }

        public string Target { get; }
        public string Attribute { get; }
        public string Relationship { get; }
    }
}
