using System.Reflection;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#pragma warning disable AV1561 // Signature contains too many parameters

namespace JsonApiDotNetCoreTests.UnitTests.Middleware;

public sealed class JsonApiMiddlewareTests
{
    // @formatter:wrap_lines false
    [Theory]
    [InlineData("HEAD", "/todoItems", EndpointKind.Primary, null, "todoItems", null, null, IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("HEAD", "/people/1", EndpointKind.Primary, "1", "people", null, null, IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("HEAD", "/todoItems/2/owner", EndpointKind.Secondary, "2", "todoItems", "people", "owner", IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("HEAD", "/todoItems/3/tags", EndpointKind.Secondary, "3", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("HEAD", "/todoItems/ABC/relationships/owner", EndpointKind.Relationship, "ABC", "todoItems", "people", "owner", IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("HEAD", "/todoItems/ABC/relationships/tags", EndpointKind.Relationship, "ABC", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems", EndpointKind.Primary, null, "todoItems", null, null, IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems/-1", EndpointKind.Primary, "-1", "todoItems", null, null, IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems/1/owner", EndpointKind.Secondary, "1", "todoItems", "people", "owner", IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems/1/tags", EndpointKind.Secondary, "1", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems/1/relationships/owner", EndpointKind.Relationship, "1", "todoItems", "people", "owner", IsCollection.No, IsReadOnly.Yes, null)]
    [InlineData("GET", "/todoItems/1/relationships/tags", EndpointKind.Relationship, "1", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.Yes, null)]
    [InlineData("POST", "/todoItems", EndpointKind.Primary, null, "todoItems", null, null, IsCollection.No, IsReadOnly.No, WriteOperationKind.CreateResource)]
    [InlineData("POST", "/todoItems/1/relationships/tags", EndpointKind.Relationship, "1", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.No, WriteOperationKind.AddToRelationship)]
    [InlineData("PATCH", "/itemTags/1", EndpointKind.Primary, "1", "itemTags", null, null, IsCollection.No, IsReadOnly.No, WriteOperationKind.UpdateResource)]
    [InlineData("PATCH", "/todoItems/1/relationships/owner", EndpointKind.Relationship, "1", "todoItems", "people", "owner", IsCollection.No, IsReadOnly.No, WriteOperationKind.SetRelationship)]
    [InlineData("PATCH", "/todoItems/1/relationships/tags", EndpointKind.Relationship, "1", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.No, WriteOperationKind.SetRelationship)]
    [InlineData("DELETE", "/todoItems/1", EndpointKind.Primary, "1", "todoItems", null, null, IsCollection.No, IsReadOnly.No, WriteOperationKind.DeleteResource)]
    [InlineData("DELETE", "/todoItems/1/relationships/tags", EndpointKind.Relationship, "1", "todoItems", "itemTags", "tags", IsCollection.Yes, IsReadOnly.No, WriteOperationKind.RemoveFromRelationship)]
    [InlineData("POST", "/operations", EndpointKind.AtomicOperations, null, null, null, null, IsCollection.No, IsReadOnly.No, null)]
    // @formatter:wrap_lines restore
    public async Task Sets_request_properties_correctly(string requestMethod, string requestPath, EndpointKind expectKind, string? expectPrimaryId,
        string? expectPrimaryResourceType, string? expectSecondaryResourceType, string? expectRelationshipName, IsCollection expectIsCollection,
        IsReadOnly expectIsReadOnly, WriteOperationKind? expectWriteOperation)
    {
        // Arrange
        var options = new JsonApiOptions();
        options.IncludeExtensions(JsonApiMediaTypeExtension.AtomicOperations);

        var request = new JsonApiRequest();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance)
            .Add<TodoItem, int>()
            .Add<Person, int>()
            .Add<ItemTag, int>()
            .Build();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        var httpContext = new DefaultHttpContext();
        FakeControllerResourceMapping controllerResourceMapping = SetupRoutes(httpContext, resourceGraph, requestMethod, requestPath);

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var contentNegotiator = new JsonApiContentNegotiator(options, httpContextAccessor);

        var middleware = new JsonApiMiddleware(null, httpContextAccessor, controllerResourceMapping, options, contentNegotiator,
            NullLogger<JsonApiMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(httpContext, request);

        // Assert
        request.Kind.Should().Be(expectKind);
        request.PrimaryId.Should().Be(expectPrimaryId);

        if (expectPrimaryResourceType == null)
        {
            request.PrimaryResourceType.Should().BeNull();
        }
        else
        {
            request.PrimaryResourceType.Should().NotBeNull();
            request.PrimaryResourceType.PublicName.Should().Be(expectPrimaryResourceType);
        }

        if (expectSecondaryResourceType == null)
        {
            request.SecondaryResourceType.Should().BeNull();
        }
        else
        {
            request.SecondaryResourceType.Should().NotBeNull();
            request.SecondaryResourceType.PublicName.Should().Be(expectSecondaryResourceType);
        }

        if (expectRelationshipName == null)
        {
            request.Relationship.Should().BeNull();
        }
        else
        {
            request.Relationship.Should().NotBeNull();
            request.Relationship.PublicName.Should().Be(expectRelationshipName);
        }

        request.IsCollection.Should().Be(expectIsCollection == IsCollection.Yes);
        request.IsReadOnly.Should().Be(expectIsReadOnly == IsReadOnly.Yes);
        request.WriteOperation.Should().Be(expectWriteOperation);
    }

    private static FakeControllerResourceMapping SetupRoutes(HttpContext httpContext, IResourceGraph resourceGraph, string requestMethod, string requestPath)
    {
        httpContext.Request.Method = requestMethod;

        var feature = new RouteValuesFeature
        {
            RouteValues =
            {
                ["controller"] = "theController",
                ["action"] = "theAction"
            }
        };

        string[] pathSegments = requestPath.Split("/", StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments.Length > 1)
        {
            feature.RouteValues["id"] = pathSegments[1];

            if (pathSegments.Length >= 3)
            {
                feature.RouteValues["relationshipName"] = pathSegments[^1];
            }
        }

        if (pathSegments.Contains("relationships"))
        {
            feature.RouteValues["action"] = "Relationship";
        }
        else if (pathSegments.Contains("operations"))
        {
            feature.RouteValues["action"] = "PostOperations";
            httpContext.Request.Headers.Accept = JsonApiMediaType.AtomicOperations.ToString();
        }

        httpContext.Features.Set<IRouteValuesFeature>(feature);

        var controllerActionDescriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = (TypeInfo)typeof(object)
        };

        httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(controllerActionDescriptor), null));

        string? resourceTypePublicName = pathSegments.Length > 0 ? pathSegments[0] : null;
        return new FakeControllerResourceMapping(resourceGraph, resourceTypePublicName);
    }

    public enum IsReadOnly
    {
        Yes,
        No
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public enum IsCollection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        Yes,
        No
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Itself)]
    private sealed class Person : Identifiable<int>;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ItemTag : Identifiable<int>
    {
        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; } = new HashSet<TodoItem>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TodoItem : Identifiable<int>
    {
        [HasOne]
        public Person? Owner { get; set; }

        [HasMany]
        public ISet<ItemTag> Tags { get; set; } = new HashSet<ItemTag>();
    }

    private sealed class FakeControllerResourceMapping(IResourceGraph resourceGraph, string? resourceTypePublicName) : IControllerResourceMapping
    {
        private readonly IResourceGraph _resourceGraph = resourceGraph;
        private readonly string? _resourceTypePublicName = resourceTypePublicName;

        public ResourceType? GetResourceTypeForController(Type? controllerType)
        {
            return _resourceTypePublicName != null ? _resourceGraph.FindResourceType(_resourceTypePublicName) : null;
        }

        public string GetControllerNameForResourceType(ResourceType? resourceType)
        {
            throw new NotImplementedException();
        }
    }
}
