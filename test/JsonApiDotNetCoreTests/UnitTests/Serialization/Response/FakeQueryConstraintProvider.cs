using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response;

internal sealed class FakeQueryConstraintProvider(params ExpressionInScope[] constraints) : IQueryConstraintProvider
{
    private readonly IReadOnlyCollection<ExpressionInScope> _constraints = constraints;

    public IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return _constraints;
    }
}
