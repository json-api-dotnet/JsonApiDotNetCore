using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Takes scoped expressions from <see cref="IQueryConstraintProvider"/>s and transforms them.
    /// </summary>
    public interface IQueryLayerComposer
    {
        /// <summary>
        /// Builds a top-level filter from constraints, used to determine total resource count.
        /// </summary>
        FilterExpression GetTopFilter();
        
        /// <summary>
        /// Collects constraints and builds a <see cref="QueryLayer"/> out of them, used to retrieve the actual resources.
        /// </summary>
        QueryLayer Compose(ResourceContext requestResource);
    }
}
