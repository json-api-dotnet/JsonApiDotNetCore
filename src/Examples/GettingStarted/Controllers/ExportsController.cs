using System.Collections.Immutable;
using System.Text.Json;
using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExportsController : ControllerBase
{
    private readonly SampleDbContext _dbContext;
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly IMetaBuilder _metaBuilder;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;

    public ExportsController(SampleDbContext dbContext, IJsonApiOptions options, IResourceGraph resourceGraph, IMetaBuilder metaBuilder,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        _dbContext = dbContext;
        _options = options;
        _resourceGraph = resourceGraph;
        _metaBuilder = metaBuilder;
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
    }

    /// <summary>
    /// Returns a JSON:API response, containing all books and their associated authors.
    /// </summary>
    [HttpGet("asJson")]
    public async Task<IActionResult> GetJsonAsync(CancellationToken cancellationToken)
    {
        List<Book> booksWithAuthors = await _dbContext.Books.Include(book => book.Author).ToListAsync(cancellationToken);

        ResourceType bookType = _resourceGraph.GetResourceType<Book>();
        RelationshipAttribute bookAuthorRelationship = bookType.GetRelationshipByPropertyName(nameof(Book.Author));

        var includeExpression =
            new IncludeExpression(ImmutableHashSet<IncludeElementExpression>.Empty.Add(new IncludeElementExpression(bookAuthorRelationship)));

        string json = ResourcesToJson(booksWithAuthors, includeExpression);
        return Ok(json);
    }

    /// <summary>
    /// Exports all books and all people to individual .json files and returns a .zip file containing them.
    /// </summary>
    [HttpGet("asZipFile")]
    public async Task<IActionResult> GetZipFileAsync(CancellationToken cancellationToken)
    {
        List<Book> books = await _dbContext.Books.ToListAsync(cancellationToken);
        List<Person> people = await _dbContext.People.ToListAsync(cancellationToken);

        var zipFileBuilder = new ZipFileBuilder();

        await WriteResourcesToZipFileAsync(books, zipFileBuilder);
        await WriteResourcesToZipFileAsync(people, zipFileBuilder);

        Stream zipStream = zipFileBuilder.Build();
        return File(zipStream, "application/zip", "Export.zip");
    }

    private async Task WriteResourcesToZipFileAsync<TResource>(IEnumerable<TResource> resources, ZipFileBuilder zipFileBuilder)
        where TResource : class, IIdentifiable
    {
        string json = ResourcesToJson(resources, null);

        ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();
        string fileName = $"{resourceType}.json";

        await zipFileBuilder.IncludeFileAsync(fileName, json);
    }

    private string ResourcesToJson<TResource>(IEnumerable<TResource> resources, IncludeExpression? include)
        where TResource : class, IIdentifiable
    {
        ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = resourceType,
            IsCollection = true,
            IsReadOnly = true
        };

        var linksBuilder = new HiddenLinksBuilder();
        var includeCache = new IncludeCache(include);
        var sparseFieldSetCache = new EveryFieldCache();
        var queryStringAccessor = new EmptyQueryStringAccessor();

        var adapter = new ResponseModelAdapter(request, _options, linksBuilder, _metaBuilder, _resourceDefinitionAccessor, includeCache, sparseFieldSetCache,
            queryStringAccessor);

        Document document = adapter.Convert(resources);
        return JsonSerializer.Serialize(document, _options.SerializerOptions);
    }

    /// <summary>
    /// Enables to explicitly set the inclusion chain to use, which is normally based on the '?include=' query string parameter.
    /// </summary>
    private sealed class IncludeCache : IEvaluatedIncludeCache
    {
        private IncludeExpression? _include;

        public IncludeCache(IncludeExpression? include)
        {
            _include = include;
        }

        public void Set(IncludeExpression include)
        {
            _include = include;
        }

        public IncludeExpression? Get()
        {
            return _include;
        }
    }

    /// <summary>
    /// Hides all JSON:API links.
    /// </summary>
    private sealed class HiddenLinksBuilder : ILinkBuilder
    {
        public TopLevelLinks? GetTopLevelLinks()
        {
            return null;
        }

        public ResourceLinks? GetResourceLinks(ResourceType resourceType, IIdentifiable resource)
        {
            return null;
        }

        public RelationshipLinks? GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource)
        {
            return null;
        }
    }

    /// <summary>
    /// Forces to return all fields (attributes and relationships), which is normally based on the '?fields=' query string parameter.
    /// </summary>
    private sealed class EveryFieldCache : ISparseFieldSetCache
    {
        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType)
        {
            return resourceType.Fields.ToImmutableHashSet();
        }

        public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
        {
            return resourceType.Attributes.ToImmutableHashSet();
        }

        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceType resourceType)
        {
            return resourceType.Fields.ToImmutableHashSet();
        }

        public void Reset()
        {
        }
    }

    /// <summary>
    /// Ignores any incoming query string parameters.
    /// </summary>
    private sealed class EmptyQueryStringAccessor : IRequestQueryStringAccessor
    {
        public IQueryCollection Query { get; } = new QueryCollection();
    }
}
