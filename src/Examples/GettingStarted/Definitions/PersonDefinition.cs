using GettingStarted.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;

namespace GettingStarted.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class PersonDefinition(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
    : JsonApiResourceDefinition<Person, int>(resourceGraph)
{
    private readonly IResourceFactory _resourceFactory = resourceFactory;

    public override FilterExpression OnApplyFilter(FilterExpression? existingFilter)
    {
        var parser = new FilterParser(_resourceFactory);
        FilterExpression isNotDeleted = parser.Parse("equals(isDeleted,'false')", ResourceType);
        FilterExpression hasBooksWithName = parser.Parse("has(books,equals(author.name,'Mary Shelley'))", ResourceType);
        FilterExpression ownsBigHouseWithFloorCount = parser.Parse("isType(house,bigHouses,equals(floorCount,'3'))", ResourceType);

        return LogicalExpression.Compose(LogicalOperator.And, isNotDeleted, hasBooksWithName, ownsBigHouseWithFloorCount, existingFilter)!;
    }
}
