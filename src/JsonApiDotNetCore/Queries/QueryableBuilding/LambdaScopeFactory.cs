using System.Linq.Expressions;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Produces lambda parameters with unique names.
/// </summary>
[PublicAPI]
public sealed class LambdaScopeFactory
{
    private readonly HashSet<string> _namesInScope = new();

    /// <summary>
    /// Finds the next unique lambda parameter name. Dispose the returned scope to release the claimed name, so it can be reused.
    /// </summary>
    public LambdaScope CreateScope(Type elementType, Expression? accessorExpression = null)
    {
        ArgumentGuard.NotNull(elementType);

        string parameterName = elementType.Name.Camelize();
        parameterName = EnsureUniqueName(parameterName);
        _namesInScope.Add(parameterName);

        return LambdaScope.Create(this, elementType, parameterName, accessorExpression);
    }

    private string EnsureUniqueName(string name)
    {
        if (!_namesInScope.Contains(name))
        {
            return name;
        }

        int counter = 1;
        string alternativeName;

        do
        {
            counter++;
            alternativeName = name + counter;
        }
        while (_namesInScope.Contains(alternativeName));

        return alternativeName;
    }

    internal void Release(LambdaScope lambdaScope)
    {
        ArgumentGuard.NotNull(lambdaScope);

        _namesInScope.Remove(lambdaScope.Parameter.Name!);
    }
}
