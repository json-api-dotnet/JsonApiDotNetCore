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

    private LambdaScope(LambdaParameterNameScope parameterNameScope, ParameterExpression parameter, Expression accessor)
    {
        _parameterNameScope = parameterNameScope;
        Parameter = parameter;
        Accessor = accessor;
    }

    public static LambdaScope Create(LambdaParameterNameFactory nameFactory, Type elementType, Expression? accessorExpression)
    {
        ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));
        ArgumentGuard.NotNull(elementType, nameof(elementType));

        LambdaParameterNameScope parameterNameScope = nameFactory.Create(elementType.Name);
        ParameterExpression parameter = Expression.Parameter(elementType, parameterNameScope.Name);
        Expression accessor = accessorExpression ?? parameter;

        return new LambdaScope(parameterNameScope, parameter, accessor);
    }

    public LambdaScope WithAccessor(Expression accessorExpression)
    {
        ArgumentGuard.NotNull(accessorExpression, nameof(accessorExpression));

        return new LambdaScope(_parameterNameScope, Parameter, accessorExpression);
    }

    public void Dispose()
    {
        _parameterNameScope.Dispose();
    }
}
