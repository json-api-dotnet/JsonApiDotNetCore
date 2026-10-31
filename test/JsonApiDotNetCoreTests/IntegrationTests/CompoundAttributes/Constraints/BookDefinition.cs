using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.Constraints;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BookDefinition(IResourceGraph resourceGraph, IJsonApiRequest request)
    : JsonApiResourceDefinition<Book, long>(resourceGraph)
{
    private readonly IJsonApiRequest _request = request;

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        if (!_request.IsReadOnly)
        {
            return existingFilter;
        }

        // Rule: hide hidden books, wherever they occur (primary, secondary and included).
        AttrAttribute isHiddenAttribute = ResourceType.GetAttributeByPropertyName(nameof(Book.IsHidden));
        var isHiddenChain = new ResourceFieldChainExpression(isHiddenAttribute);

        FilterExpression isNotHidden = new ComparisonExpression(ComparisonOperator.Equals, isHiddenChain, new LiteralConstantExpression(false));

        return LogicalExpression.Compose(LogicalOperator.And, existingFilter, isNotHidden);
    }
}
