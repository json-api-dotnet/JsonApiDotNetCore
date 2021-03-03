using FluentAssertions;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.UnitTests.Links
{
    public sealed class LinkInclusionTests
    {
        [Theory]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.NotConfigured, LinkTypes.None)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.None, LinkTypes.None)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, LinkTypes.Self)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, LinkTypes.Related)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Paging, LinkTypes.Paging)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.All, LinkTypes.All)]
        [InlineData(LinkTypes.None, LinkTypes.NotConfigured, LinkTypes.None)]
        [InlineData(LinkTypes.None, LinkTypes.None, LinkTypes.None)]
        [InlineData(LinkTypes.None, LinkTypes.Self, LinkTypes.None)]
        [InlineData(LinkTypes.None, LinkTypes.Related, LinkTypes.None)]
        [InlineData(LinkTypes.None, LinkTypes.Paging, LinkTypes.None)]
        [InlineData(LinkTypes.None, LinkTypes.All, LinkTypes.None)]
        [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, LinkTypes.Self)]
        [InlineData(LinkTypes.Self, LinkTypes.None, LinkTypes.Self)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, LinkTypes.Self)]
        [InlineData(LinkTypes.Self, LinkTypes.Related, LinkTypes.Self)]
        [InlineData(LinkTypes.Self, LinkTypes.Paging, LinkTypes.Self)]
        [InlineData(LinkTypes.Self, LinkTypes.All, LinkTypes.Self)]
        [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, LinkTypes.Related)]
        [InlineData(LinkTypes.Related, LinkTypes.None, LinkTypes.Related)]
        [InlineData(LinkTypes.Related, LinkTypes.Self, LinkTypes.Related)]
        [InlineData(LinkTypes.Related, LinkTypes.Related, LinkTypes.Related)]
        [InlineData(LinkTypes.Related, LinkTypes.Paging, LinkTypes.Related)]
        [InlineData(LinkTypes.Related, LinkTypes.All, LinkTypes.Related)]
        [InlineData(LinkTypes.Paging, LinkTypes.NotConfigured, LinkTypes.Paging)]
        [InlineData(LinkTypes.Paging, LinkTypes.None, LinkTypes.Paging)]
        [InlineData(LinkTypes.Paging, LinkTypes.Self, LinkTypes.Paging)]
        [InlineData(LinkTypes.Paging, LinkTypes.Related, LinkTypes.Paging)]
        [InlineData(LinkTypes.Paging, LinkTypes.Paging, LinkTypes.Paging)]
        [InlineData(LinkTypes.Paging, LinkTypes.All, LinkTypes.Paging)]
        [InlineData(LinkTypes.All, LinkTypes.NotConfigured, LinkTypes.All)]
        [InlineData(LinkTypes.All, LinkTypes.None, LinkTypes.All)]
        [InlineData(LinkTypes.All, LinkTypes.Self, LinkTypes.All)]
        [InlineData(LinkTypes.All, LinkTypes.Related, LinkTypes.All)]
        [InlineData(LinkTypes.All, LinkTypes.Paging, LinkTypes.All)]
        [InlineData(LinkTypes.All, LinkTypes.All, LinkTypes.All)]
        public void Applies_cascading_settings_for_top_level_links(LinkTypes linksInResourceContext, LinkTypes linksInOptions, LinkTypes expected)
        {
            // Arrange
            var exampleResourceContext = new ResourceContext
            {
                PublicName = nameof(ExampleResource),
                ResourceType = typeof(ExampleResource),
                TopLevelLinks = linksInResourceContext
            };

            var resourceGraph = new ResourceGraph(exampleResourceContext.AsArray());

            var request = new JsonApiRequest
            {
                PrimaryResource = exampleResourceContext,
                PrimaryId = "1",
                IsCollection = true,
                Kind = EndpointKind.Relationship,
                Relationship = new HasOneAttribute()
            };

            var paginationContext = new PaginationContext
            {
                PageSize = new PageSize(1),
                PageNumber = new PageNumber(2),
                TotalResourceCount = 10
            };

            var queryStringAccessor = new EmptyRequestQueryStringAccessor();

            var options = new JsonApiOptions
            {
                TopLevelLinks = linksInOptions
            };

            var linkBuilder = new LinkBuilder(options, request, paginationContext, resourceGraph, queryStringAccessor);

            // Act
            TopLevelLinks topLevelLinks = linkBuilder.GetTopLevelLinks();

            // Assert
            if (expected == LinkTypes.None)
            {
                topLevelLinks.Should().BeNull();
            }
            else
            {
                if (expected.HasFlag(LinkTypes.Self))
                {
                    topLevelLinks.Self.Should().NotBeNull();
                }
                else
                {
                    topLevelLinks.Self.Should().BeNull();
                }

                if (expected.HasFlag(LinkTypes.Related))
                {
                    topLevelLinks.Related.Should().NotBeNull();
                }
                else
                {
                    topLevelLinks.Related.Should().BeNull();
                }

                if (expected.HasFlag(LinkTypes.Paging))
                {
                    topLevelLinks.First.Should().NotBeNull();
                    topLevelLinks.Last.Should().NotBeNull();
                    topLevelLinks.Prev.Should().NotBeNull();
                    topLevelLinks.Next.Should().NotBeNull();
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
        public void Applies_cascading_settings_for_resource_links(LinkTypes linksInResourceContext, LinkTypes linksInOptions, LinkTypes expected)
        {
            // Arrange
            var exampleResourceContext = new ResourceContext
            {
                PublicName = nameof(ExampleResource),
                ResourceType = typeof(ExampleResource),
                ResourceLinks = linksInResourceContext
            };

            var resourceGraph = new ResourceGraph(exampleResourceContext.AsArray());

            var request = new JsonApiRequest();

            var paginationContext = new PaginationContext();

            var queryStringAccessor = new EmptyRequestQueryStringAccessor();

            var options = new JsonApiOptions
            {
                ResourceLinks = linksInOptions
            };

            var linkBuilder = new LinkBuilder(options, request, paginationContext, resourceGraph, queryStringAccessor);

            // Act
            ResourceLinks resourceLinks = linkBuilder.GetResourceLinks(nameof(ExampleResource), "id");

            // Assert
            if (expected == LinkTypes.Self)
            {
                resourceLinks.Self.Should().NotBeNull();
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
        public void Applies_cascading_settings_for_relationship_links(LinkTypes linksInRelationshipAttribute, LinkTypes linksInResourceContext,
            LinkTypes linksInOptions, LinkTypes expected)
        {
            // Arrange
            var exampleResourceContext = new ResourceContext
            {
                PublicName = nameof(ExampleResource),
                ResourceType = typeof(ExampleResource),
                RelationshipLinks = linksInResourceContext
            };

            var resourceGraph = new ResourceGraph(exampleResourceContext.AsArray());

            var request = new JsonApiRequest();

            var paginationContext = new PaginationContext();

            var queryStringAccessor = new EmptyRequestQueryStringAccessor();

            var options = new JsonApiOptions
            {
                RelationshipLinks = linksInOptions
            };

            var linkBuilder = new LinkBuilder(options, request, paginationContext, resourceGraph, queryStringAccessor);

            var relationship = new HasOneAttribute
            {
                Links = linksInRelationshipAttribute
            };

            // Act
            RelationshipLinks relationshipLinks = linkBuilder.GetRelationshipLinks(relationship, new ExampleResource());

            // Assert
            if (expected == LinkTypes.None)
            {
                relationshipLinks.Should().BeNull();
            }
            else
            {
                if (expected.HasFlag(LinkTypes.Self))
                {
                    relationshipLinks.Self.Should().NotBeNull();
                }
                else
                {
                    relationshipLinks.Self.Should().BeNull();
                }

                if (expected.HasFlag(LinkTypes.Related))
                {
                    relationshipLinks.Related.Should().NotBeNull();
                }
                else
                {
                    relationshipLinks.Related.Should().BeNull();
                }
            }
        }

        private sealed class EmptyRequestQueryStringAccessor : IRequestQueryStringAccessor
        {
            public IQueryCollection Query { get; } = new QueryCollection();
        }

        private sealed class ExampleResource : Identifiable
        {
        }
    }
}
