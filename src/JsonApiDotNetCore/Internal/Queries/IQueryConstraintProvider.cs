using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal.Queries
{
    public interface IQueryConstraintProvider
    {
        public IReadOnlyCollection<ExpressionInScope> GetConstraints();
    }
}
