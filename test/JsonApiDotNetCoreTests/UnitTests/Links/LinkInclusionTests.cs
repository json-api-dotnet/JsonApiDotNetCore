using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Links;

public sealed class LinkInclusionTests
{
    [Theory]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Pagination, LinkTypes.Pagination)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Pagination, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Pagination, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Pagination, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.Pagination, LinkTypes.NotConfigured, LinkTypes.Pagination)]
    [InlineData(LinkTypes.Pagination, LinkTypes.None, LinkTypes.Pagination)]
    [InlineData(LinkTypes.Pagination, LinkTypes.Self, LinkTypes.Pagination)]
    [InlineData(LinkTypes.Pagination, LinkTypes.Related, LinkTypes.Pagination)]
    [InlineData(LinkTypes.Pagination, LinkTypes.Pagination, LinkTypes.Pagination)]
    [InlineData(LinkTypes.Pagination, LinkTypes.All, LinkTypes.Pagination)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Pagination, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.All)]
    public void Applies_cascading_settings_for_top_level_links(LinkTypes linksInResourceType, LinkTypes linksInOptions, LinkTypes expected)
    {
        // Arrange
        var exampleResourceType = new ResourceType(nameof(ExampleResource), typeof(ExampleResource), typeof(int), linksInResourceType);

        var options = new JsonApiOptions
        {
            TopLevelLinks = linksInOptions
        };

        var request = new JsonApiRequest
        {
            PrimaryResourceType = exampleResourceType,
            PrimaryId = "1",
            IsCollection = true,
            Kind = EndpointKind.Relationship,
            Relationship = new HasOneAttribute
            {
                Type = exampleResourceType
            }
        };

        var paginationContext = new PaginationContext
        {
            PageSize = new PageSize(1),
            PageNumber = new PageNumber(2),
            TotalResourceCount = 10
        };

        var httpContextAccessor = new FakeHttpContextAccessor();
        var linkGenerator = new FakeLinkGenerator();
        var controllerResourceMapping = new FakeControllerResourceMapping();
        var paginationParser = new PaginationParser();
        var linkBuilder = new LinkBuilder(options, request, paginationContext, httpContextAccessor, linkGenerator, controllerResourceMapping, paginationParser);

        // Act
        TopLevelLinks? topLevelLinks = linkBuilder.GetTopLevelLinks();

        // Assert
        if (expected == LinkTypes.None)
        {
            topLevelLinks.Should().BeNull();
        }
        else
        {
            topLevelLinks.ShouldNotBeNull();

            if (expected.HasFlag(LinkTypes.Self))
            {
                topLevelLinks.Self.ShouldNotBeNull();
            }
            else
            {
                topLevelLinks.Self.Should().BeNull();
            }

            if (expected.HasFlag(LinkTypes.Related))
            {
                topLevelLinks.Related.ShouldNotBeNull();
            }
            else
            {
                topLevelLinks.Related.Should().BeNull();
            }

            if (expected.HasFlag(LinkTypes.Pagination))
            {
                topLevelLinks.First.ShouldNotBeNull();
                topLevelLinks.Last.ShouldNotBeNull();
                topLevelLinks.Prev.ShouldNotBeNull();
                topLevelLinks.Next.ShouldNotBeNull();
            }
            else
            {
                topLevelLinks.First.Should().BeNull();
                topLevelLinks.Last.Should().BeNull();
                topLevelLinks.Prev.Should().BeNull();
                topLevelLinks.Next.Should().BeNull();
            }
        }
    }

    [Theory]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.Self)]
    public void Applies_cascading_settings_for_resource_links(LinkTypes linksInResourceType, LinkTypes linksInOptions, LinkTypes expected)
    {
        // Arrange
        var exampleResourceType = new ResourceType(nameof(ExampleResource), typeof(ExampleResource), typeof(int), resourceLinks: linksInResourceType);

        var options = new JsonApiOptions
        {
            ResourceLinks = linksInOptions
        };

        var request = new JsonApiRequest();
        var paginationContext = new PaginationContext();
        var httpContextAccessor = new FakeHttpContextAccessor();
        var linkGenerator = new FakeLinkGenerator();
        var controllerResourceMapping = new FakeControllerResourceMapping();
        var paginationParser = new PaginationParser();
        var linkBuilder = new LinkBuilder(options, request, paginationContext, httpContextAccessor, linkGenerator, controllerResourceMapping, paginationParser);

        // Act
        ResourceLinks? resourceLinks = linkBuilder.GetResourceLinks(exampleResourceType, new ExampleResource());

        // Assert
        if (expected == LinkTypes.Self)
        {
            resourceLinks.ShouldNotBeNull();
            resourceLinks.Self.ShouldNotBeNull();
        }
        else
        {
            resourceLinks.Should().BeNull();
        }
    }

    [Theory]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.None, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.Self, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.Related, LinkTypes.None)]
    [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.All, LinkTypes.None)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.None, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.Self, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.Related, LinkTypes.Self)]
    [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.All, LinkTypes.Self)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.None, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.Self, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.Related, LinkTypes.Related)]
    [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.All, LinkTypes.Related)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.All, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.None, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.Self, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.Related, LinkTypes.All)]
    [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.All, LinkTypes.All)]
    public void Applies_cascading_settings_for_relationship_links(LinkTypes linksInRelationshipAttribute, LinkTypes linksInResourceType,
        LinkTypes linksInOptions, LinkTypes expected)
    {
        // Arrange
        var exampleResourceType = new ResourceType(nameof(ExampleResource), typeof(ExampleResource), typeof(int), relationshipLinks: linksInResourceType);

        var options = new JsonApiOptions
        {
            RelationshipLinks = linksInOptions
        };

        var request = new JsonApiRequest();
        var paginationContext = new PaginationContext();
        var httpContextAccessor = new FakeHttpContextAccessor();
        var linkGenerator = new FakeLinkGenerator();
        var controllerResourceMapping = new FakeControllerResourceMapping();
        var paginationParser = new PaginationParser();
        var linkBuilder = new LinkBuilder(options, request, paginationContext, httpContextAccessor, linkGenerator, controllerResourceMapping, paginationParser);

        var relationship = new HasOneAttribute
        {
            Links = linksInRelationshipAttribute,
            Type = exampleResourceType
        };

        // Act
        RelationshipLinks? relationshipLinks = linkBuilder.GetRelationshipLinks(relationship, new ExampleResource());

        // Assert
        if (expected == LinkTypes.None)
        {
            relationshipLinks.Should().BeNull();
        }
        else
        {
            relationshipLinks.ShouldNotBeNull();

            if (expected.HasFlag(LinkTypes.Self))
            {
                relationshipLinks.Self.ShouldNotBeNull();
            }
            else
            {
                relationshipLinks.Self.Should().BeNull();
            }

            if (expected.HasFlag(LinkTypes.Related))
            {
                relationshipLinks.Related.ShouldNotBeNull();
            }
            else
            {
                relationshipLinks.Related.Should().BeNull();
            }
        }
    }

    private sealed class ExampleResource : Identifiable<int>
    {
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost")
            }
        };
    }

    private sealed class FakeControllerResourceMapping : IControllerResourceMapping
    {
        public ResourceType GetResourceTypeForController(Type? controllerType)
        {
            throw new NotImplementedException();
        }

        public string? GetControllerNameForResourceType(ResourceType? resourceType)
        {
            return resourceType == null ? null : $"{resourceType.PublicName.Pascalize()}Controller";
        }
    }

    private sealed class FakeLinkGenerator : LinkGenerator
    {
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
        public override string GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
            RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = new(), LinkOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public override string GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = new(),
            FragmentString fragment = new(), LinkOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public override string GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
            RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null,
            FragmentString fragment = new(), LinkOptions? options = null)
        {
            return "https://domain.com/some/path";
        }

        public override string GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host,
            PathString pathBase = new(), FragmentString fragment = new(), LinkOptions? options = null)
        {
            throw new NotImplementedException();
        }
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    }
}
