using System.Collections.Immutable;
using System.ComponentModel;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class WheelSortDefinition : JsonApiResourceDefinition<Wheel, long>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WheelSortDefinition(IResourceGraph resourceGraph, IHttpContextAccessor httpContextAccessor)
        : base(resourceGraph)
    {
        ArgumentGuard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
    }

    public override SortExpression? OnApplySort(SortExpression? existingSort)
    {
        if (_httpContextAccessor.HttpContext!.Request.Query["autoSort"] == "expr")
        {
            return CreateSortFromExpressionSyntax();
        }

        if (_httpContextAccessor.HttpContext!.Request.Query["autoSort"] == "lambda")
        {
            return CreateSortFromLambdaSyntax();
        }

        return existingSort;
    }

    private SortExpression CreateSortFromExpressionSyntax()
    {
        AttrAttribute paintColorAttribute = ResourceGraph.GetResourceType<ChromeWheel>().GetAttributeByPropertyName(nameof(ChromeWheel.PaintColor));
        AttrAttribute hasTubeAttribute = ResourceGraph.GetResourceType<CarbonWheel>().GetAttributeByPropertyName(nameof(CarbonWheel.HasTube));

        var cylinderCountChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(
            ResourceGraph.GetResourceType<Wheel>().GetRelationshipByPropertyName(nameof(Wheel.Vehicle)),
            ResourceGraph.GetResourceType<Car>().GetRelationshipByPropertyName(nameof(Car.Engine)),
            ResourceGraph.GetResourceType<GasolineEngine>().GetRelationshipByPropertyName(nameof(GasolineEngine.Cylinders))));

        return new SortExpression(new[]
        {
            new SortElementExpression(new ResourceFieldChainExpression(paintColorAttribute), true),
            new SortElementExpression(new ResourceFieldChainExpression(hasTubeAttribute), false),
            new SortElementExpression(new CountExpression(cylinderCountChain), true)
        }.ToImmutableArray());
    }

    private SortExpression CreateSortFromLambdaSyntax()
    {
        return CreateSortExpressionFromLambda(new PropertySortOrder
        {
            (wheel => (wheel as ChromeWheel)!.PaintColor, ListSortDirection.Ascending),
            (wheel => ((CarbonWheel)wheel).HasTube, ListSortDirection.Descending),
            (wheel => ((GasolineEngine)((Car)wheel.Vehicle!).Engine).Cylinders.Count, ListSortDirection.Ascending)
        });
    }
}
