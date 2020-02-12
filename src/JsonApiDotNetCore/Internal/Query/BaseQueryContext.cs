using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// A context class that provides extra meta data for a <typeparamref name="TQuery"/>
    /// that is used when applying url queries internally.
    /// </summary>
    public abstract class BaseQueryContext<TQuery> where TQuery : BaseQuery
    {
        protected BaseQueryContext(TQuery query)
        {
            Query = query;
        }

        public bool IsCustom { get; internal set; }
        public AttrAttribute Attribute { get; internal set; }
        public RelationshipAttribute Relationship { get; internal set; }
        public bool IsAttributeOfRelationship => Relationship != null;

        public TQuery Query { get; }

        public string GetPropertyPath()
        {
            if (IsAttributeOfRelationship)
                return string.Format("{0}.{1}", Relationship.InternalRelationshipName, Attribute.PropertyInfo.Name);

            return Attribute.PropertyInfo.Name;
        }
    }
}
