namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Provides constraints (such as filters, sorting, pagination, sparse fieldsets and inclusions) to be applied on a data set.
    /// </summary>
    public interface IQueryConstraintProvider
    {
        /// <summary>
        /// Returns a set of scoped expressions.
        /// </summary>
        public IReadOnlyCollection<ExpressionInScope> GetConstraints();
    }
}
