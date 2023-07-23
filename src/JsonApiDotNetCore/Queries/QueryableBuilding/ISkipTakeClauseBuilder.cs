using System.Linq.Expressions;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Transforms <see cref="PaginationExpression" /> into <see cref="Queryable.Skip{TSource}" /> and
/// <see cref="Queryable.Take{TSource}(IQueryable{TSource},int)" /> calls.
/// </summary>
/// <remarks>
/// Types that implement this interface are stateless by design. Existing instances are reused recursively (perhaps this one not today, but that may
/// change), so don't store mutable state in private fields when implementing this interface or deriving from the built-in implementations. To pass
/// custom state, use the <see cref="QueryClauseBuilderContext.State" /> property. The only private field allowed is a stack where you push/pop state, so
/// it works recursively.
/// </remarks>
public interface ISkipTakeClauseBuilder
{
    Expression ApplySkipTake(PaginationExpression expression, QueryClauseBuilderContext context);
}
