using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

/// <summary>
/// Hides links that are never returned.
/// </summary>
/// <remarks>
/// Tradeoff: Special-casing links per resource type and per relationship means an explosion of expanded types, only because the links visibility may
/// vary. Furthermore, relationship links fallback to their left-type resource, whereas we generate right-type component schemas for relationships. To
/// keep it simple, we take the union of exposed links on resource types and relationships. Only what's not in this unification gets hidden. For example,
/// when options == None, typeof(Blogs) == Self, and typeof(Posts) == Related, we'll keep Self | Related for both Blogs and Posts, and remove any other
/// links.
/// </remarks>
internal sealed class LinksVisibilitySchemaGenerator
{
    private const LinkTypes ResourceTopLinkTypes = LinkTypes.Self | LinkTypes.DescribedBy;
    private const LinkTypes ResourceCollectionTopLinkTypes = LinkTypes.Self | LinkTypes.DescribedBy | LinkTypes.Pagination;
    private const LinkTypes ResourceIdentifierTopLinkTypes = LinkTypes.Self | LinkTypes.Related | LinkTypes.DescribedBy;
    private const LinkTypes ResourceIdentifierCollectionTopLinkTypes = LinkTypes.Self | LinkTypes.Related | LinkTypes.DescribedBy | LinkTypes.Pagination;
    private const LinkTypes ErrorTopLinkTypes = LinkTypes.Self | LinkTypes.DescribedBy;
    private const LinkTypes RelationshipLinkTypes = LinkTypes.Self | LinkTypes.Related;
    private const LinkTypes ResourceLinkTypes = LinkTypes.Self;

    private static readonly Dictionary<Type, LinkTypes> LinksInJsonApiSchemaTypes = new()
    {
        [typeof(NullableSecondaryResponseDocument<>)] = ResourceTopLinkTypes,
        [typeof(PrimaryResponseDocument<>)] = ResourceTopLinkTypes,
        [typeof(SecondaryResponseDocument<>)] = ResourceTopLinkTypes,
        [typeof(CollectionResponseDocument<>)] = ResourceCollectionTopLinkTypes,
        [typeof(IdentifierResponseDocument<>)] = ResourceIdentifierTopLinkTypes,
        [typeof(NullableIdentifierResponseDocument<>)] = ResourceIdentifierTopLinkTypes,
        [typeof(IdentifierCollectionResponseDocument<>)] = ResourceIdentifierCollectionTopLinkTypes,
        [typeof(ErrorResponseDocument)] = ErrorTopLinkTypes,
        [typeof(OperationsResponseDocument)] = ResourceTopLinkTypes,
        [typeof(NullableToOneInResponse<>)] = RelationshipLinkTypes,
        [typeof(ToManyInResponse<>)] = RelationshipLinkTypes,
        [typeof(ToOneInResponse<>)] = RelationshipLinkTypes,
        [typeof(DataInResponse<>)] = ResourceLinkTypes
    };

    private static readonly Dictionary<LinkTypes, List<string>> LinkTypeToPropertyNamesMap = new()
    {
        [LinkTypes.Self] = ["self"],
        [LinkTypes.Related] = ["related"],
        [LinkTypes.DescribedBy] = ["describedby"],
        [LinkTypes.Pagination] =
        [
            "first",
            "last",
            "prev",
            "next"
        ]
    };

    private readonly Lazy<LinksVisibility> _lazyLinksVisibility;

    public LinksVisibilitySchemaGenerator(IJsonApiOptions options, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _lazyLinksVisibility = new Lazy<LinksVisibility>(() => new LinksVisibility(options, resourceGraph), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public void UpdateSchemaForTopLevel(Type schemaType, OpenApiSchema inlineSchemaForLinksContainer, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(inlineSchemaForLinksContainer);

        Type lookupType = schemaType.ConstructedToOpenType();

        if (LinksInJsonApiSchemaTypes.TryGetValue(lookupType, out LinkTypes possibleLinkTypes))
        {
            UpdateLinksProperty(inlineSchemaForLinksContainer, _lazyLinksVisibility.Value.TopLevelLinks, possibleLinkTypes, schemaRepository);
        }
    }

    public void UpdateSchemaForResource(ResourceSchemaType resourceSchemaType, OpenApiSchema inlineSchemaForResourceData, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(resourceSchemaType);
        ArgumentNullException.ThrowIfNull(inlineSchemaForResourceData);

        if (LinksInJsonApiSchemaTypes.TryGetValue(resourceSchemaType.SchemaOpenType, out LinkTypes possibleLinkTypes))
        {
            UpdateLinksProperty(inlineSchemaForResourceData, _lazyLinksVisibility.Value.ResourceLinks, possibleLinkTypes, schemaRepository);
        }
    }

    public void UpdateSchemaForRelationship(Type schemaType, OpenApiSchema inlineSchemaForRelationship, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(inlineSchemaForRelationship);

        Type lookupType = schemaType.ConstructedToOpenType();

        if (LinksInJsonApiSchemaTypes.TryGetValue(lookupType, out LinkTypes possibleLinkTypes))
        {
            UpdateLinksProperty(inlineSchemaForRelationship, _lazyLinksVisibility.Value.RelationshipLinks, possibleLinkTypes, schemaRepository);
        }
    }

    private void UpdateLinksProperty(OpenApiSchema inlineSchemaForLinksContainer, LinkTypes visibleLinkTypes, LinkTypes possibleLinkTypes,
        SchemaRepository schemaRepository)
    {
        if (inlineSchemaForLinksContainer.Properties != null)
        {
            OpenApiSchemaReference referenceSchemaForLinks =
                inlineSchemaForLinksContainer.Properties[JsonApiPropertyName.Links].UnwrapLastExtendedSchema().AsReferenceSchema();

            if ((visibleLinkTypes & possibleLinkTypes) == 0)
            {
                inlineSchemaForLinksContainer.Required?.Remove(JsonApiPropertyName.Links);
                inlineSchemaForLinksContainer.Properties.Remove(JsonApiPropertyName.Links);

                schemaRepository.Schemas.Remove(referenceSchemaForLinks.GetReferenceId());
            }
            else if (visibleLinkTypes != possibleLinkTypes)
            {
                string linksSchemaId = referenceSchemaForLinks.GetReferenceId();

                if (schemaRepository.Schemas.TryGetValue(linksSchemaId, out IOpenApiSchema? schemaForLinks))
                {
                    OpenApiSchema inlineSchemaForLinks = schemaForLinks.AsInlineSchema();
                    UpdateLinkProperties(inlineSchemaForLinks, visibleLinkTypes);
                }
            }
        }
    }

    private void UpdateLinkProperties(OpenApiSchema inlineSchemaForLinks, LinkTypes availableLinkTypes)
    {
        foreach (string propertyName in LinkTypeToPropertyNamesMap.Where(pair => !availableLinkTypes.HasFlag(pair.Key)).SelectMany(pair => pair.Value))
        {
            inlineSchemaForLinks.Required?.Remove(propertyName);
            inlineSchemaForLinks.Properties?.Remove(propertyName);
        }
    }

    private sealed class LinksVisibility
    {
        public LinkTypes TopLevelLinks { get; }
        public LinkTypes ResourceLinks { get; }
        public LinkTypes RelationshipLinks { get; }

        public LinksVisibility(IJsonApiOptions options, IResourceGraph resourceGraph)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(resourceGraph);

            var unionTopLevelLinks = LinkTypes.None;
            var unionResourceLinks = LinkTypes.None;
            var unionRelationshipLinks = LinkTypes.None;

            foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
            {
                LinkTypes topLevelLinks = GetTopLevelLinks(resourceType, options);
                unionTopLevelLinks |= topLevelLinks;

                LinkTypes resourceLinks = GetResourceLinks(resourceType, options);
                unionResourceLinks |= resourceLinks;

                LinkTypes relationshipLinks = GetRelationshipLinks(resourceType, options);
                unionRelationshipLinks |= relationshipLinks;
            }

            TopLevelLinks = Normalize(unionTopLevelLinks);
            ResourceLinks = Normalize(unionResourceLinks);
            RelationshipLinks = Normalize(unionRelationshipLinks);
        }

        private LinkTypes GetTopLevelLinks(ResourceType resourceType, IJsonApiOptions options)
        {
            return resourceType.TopLevelLinks != LinkTypes.NotConfigured ? resourceType.TopLevelLinks :
                options.TopLevelLinks == LinkTypes.NotConfigured ? LinkTypes.None : options.TopLevelLinks;
        }

        private LinkTypes GetResourceLinks(ResourceType resourceType, IJsonApiOptions options)
        {
            return resourceType.ResourceLinks != LinkTypes.NotConfigured ? resourceType.ResourceLinks :
                options.ResourceLinks == LinkTypes.NotConfigured ? LinkTypes.None : options.ResourceLinks;
        }

        private LinkTypes GetRelationshipLinks(ResourceType resourceType, IJsonApiOptions options)
        {
            LinkTypes unionRelationshipLinks = resourceType.RelationshipLinks != LinkTypes.NotConfigured ? resourceType.RelationshipLinks :
                options.RelationshipLinks == LinkTypes.NotConfigured ? LinkTypes.None : options.RelationshipLinks;

            foreach (RelationshipAttribute relationship in resourceType.Relationships)
            {
                LinkTypes relationshipLinks = relationship.Links != LinkTypes.NotConfigured ? relationship.Links :
                    relationship.LeftType.RelationshipLinks != LinkTypes.NotConfigured ? relationship.LeftType.RelationshipLinks :
                    options.RelationshipLinks == LinkTypes.NotConfigured ? LinkTypes.None : options.RelationshipLinks;

                unionRelationshipLinks |= relationshipLinks;
            }

            return unionRelationshipLinks;
        }

        private static LinkTypes Normalize(LinkTypes linkTypes)
        {
            return linkTypes != LinkTypes.None ? linkTypes & ~LinkTypes.None : linkTypes;
        }
    }
}
