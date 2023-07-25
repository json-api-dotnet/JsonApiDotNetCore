using System.Linq.Expressions;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// A scoped lambda expression with a unique name. Disposing the instance releases the claimed name, so it can be reused.
/// </summary>
[PublicAPI]
public sealed class LambdaScope : IDisposable
{
    private readonly LambdaScopeFactory _owner;

    /// <summary>
    /// Gets the lambda parameter. For example, 'person' in: person => person.Account.Name == "Joe".
    /// </summary>
    public ParameterExpression Parameter { get; }

    /// <summary>
    /// Gets the lambda accessor. For example, 'person.Account' in: person => person.Account.Name == "Joe".
    /// </summary>
    public Expression Accessor { get; }

    private LambdaScope(LambdaScopeFactory owner, ParameterExpression parameter, Expression accessor)
    {
        _owner = owner;
        Parameter = parameter;
        Accessor = accessor;
    }

    internal static LambdaScope Create(LambdaScopeFactory owner, Type elementType, string parameterName, Expression? accessorExpression = null)
    {
        ArgumentGuard.NotNull(owner);
        ArgumentGuard.NotNull(elementType);
        ArgumentGuard.NotNullNorEmpty(parameterName);

        ParameterExpression parameter = Expression.Parameter(elementType, parameterName);
        Expression accessor = accessorExpression ?? parameter;

        return new LambdaScope(owner, parameter, accessor);
    }

    public LambdaScope WithAccessor(Expression accessorExpression)
    {
        ArgumentGuard.NotNull(accessorExpression);

        return new LambdaScope(_owner, Parameter, accessorExpression);
    }

    public void Dispose()
    {
        _owner.Release(this);
    }
}
