using System.Linq.Expressions;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Contains details on a lambda expression, such as the name of the selector "x" in "x => x.Name".
/// </summary>
[PublicAPI]
public sealed class LambdaScope : IDisposable
{
    private readonly LambdaParameterNameScope _parameterNameScope;

    public ParameterExpression Parameter { get; }
    public Expression Accessor { get; }

    public LambdaScope(LambdaParameterNameFactory nameFactory, Type elementType, Expression? accessorExpression)
    {
        ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));
        ArgumentGuard.NotNull(elementType, nameof(elementType));

        _parameterNameScope = nameFactory.Create(elementType.Name);
        Parameter = Expression.Parameter(elementType, _parameterNameScope.Name);

        Accessor = accessorExpression ?? Parameter;
    }

    public void Dispose()
    {
        _parameterNameScope.Dispose();
    }
}
