using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;

namespace OpenApiTests.ResourceInheritance.OnlyOperations;

public static class ServiceCollectionExtensions
{
    public static void RegisterTestOperationFilter(this IServiceCollection services)
    {
        services.AddSingleton<IAtomicOperationFilter>(serviceProvider =>
        {
            var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();
            var operationFilter = new FakeAtomicOperationFilter();

            ResourceType residenceResourceType = resourceGraph.GetResourceType<Residence>();
            operationFilter.Register(residenceResourceType, JsonApiEndpoints.Post | JsonApiEndpoints.Patch);

            ResourceType familyHomeResourceType = resourceGraph.GetResourceType<FamilyHome>();
            operationFilter.Register(familyHomeResourceType, JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship);

            ResourceType mansionResourceType = resourceGraph.GetResourceType<Mansion>();
            operationFilter.Register(mansionResourceType, JsonApiEndpoints.DeleteRelationship);

            ResourceType roomResourceType = resourceGraph.GetResourceType<Room>();
            operationFilter.Register(roomResourceType, JsonApiEndpoints.Post | JsonApiEndpoints.PatchRelationship);

            return operationFilter;
        });
    }
}
