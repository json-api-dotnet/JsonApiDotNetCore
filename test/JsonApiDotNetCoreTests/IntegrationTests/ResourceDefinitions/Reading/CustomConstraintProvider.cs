using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

/// <summary>
/// Provides constraints of both scoped kinds: <see cref="RelationshipChainExpressionInScope" /> and <see cref="ResourceTypeExpressionInScope" />. Does
/// nothing unless enabled by a test.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CustomConstraintProvider(IResourceGraph resourceGraph, CustomConstraintSettings settings) : IQueryConstraintProvider
{
    private readonly IResourceGraph _resourceGraph = resourceGraph;
    private readonly CustomConstraintSettings _settings = settings;

    public IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        if (!_settings.IsEnabled)
        {
            return [];
        }

        ResourceType starType = _resourceGraph.GetResourceType<Star>();
        ResourceType planetType = _resourceGraph.GetResourceType<Planet>();

        AttrAttribute isVisibleFromEarthAttribute = starType.GetAttributeByPropertyName(nameof(Star.IsVisibleFromEarth));
        RelationshipAttribute planetsRelationship = starType.GetRelationshipByPropertyName(nameof(Star.Planets));
        AttrAttribute hasRingSystemAttribute = planetType.GetAttributeByPropertyName(nameof(Planet.HasRingSystem));
        AttrAttribute publicNameAttribute = planetType.GetAttributeByPropertyName(nameof(Planet.PublicName));

        FilterExpression isVisibleFromEarth = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(isVisibleFromEarthAttribute),
            new LiteralConstantExpression(true));

        FilterExpression hasRingSystem = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(hasRingSystemAttribute),
            new LiteralConstantExpression(true));

        var planetFieldSet = new SparseFieldSetExpression(
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty.Add(publicNameAttribute,
                new SparseFieldSetElementExpression(publicNameAttribute)));

        return
        [
            new RelationshipChainExpressionInScope(null, isVisibleFromEarth),
            new RelationshipChainExpressionInScope(new ResourceFieldChainExpression(planetsRelationship), hasRingSystem),
            new ResourceTypeExpressionInScope(planetType, planetFieldSet)
        ];
    }
}
