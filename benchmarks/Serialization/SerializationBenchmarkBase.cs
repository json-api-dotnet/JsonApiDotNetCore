using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.Serialization;

public abstract class SerializationBenchmarkBase
{
    protected readonly JsonSerializerOptions SerializerWriteOptions;
    protected readonly IResponseModelAdapter ResponseModelAdapter;
    protected readonly IResourceGraph ResourceGraph;

    protected SerializationBenchmarkBase()
    {
        var options = new JsonApiOptions
        {
            SerializerOptions =
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            }
        };

        ResourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<OutgoingResource, int>().Build();
        SerializerWriteOptions = ((IJsonApiOptions)options).SerializerWriteOptions;

        // ReSharper disable VirtualMemberCallInConstructor
        JsonApiRequest request = CreateJsonApiRequest(ResourceGraph);
        IEvaluatedIncludeCache evaluatedIncludeCache = CreateEvaluatedIncludeCache(ResourceGraph);
        // ReSharper restore VirtualMemberCallInConstructor

        var linkBuilder = new FakeLinkBuilder();
        var metaBuilder = new FakeMetaBuilder();
        IQueryConstraintProvider[] constraintProviders = Array.Empty<IQueryConstraintProvider>();
        var resourceDefinitionAccessor = new FakeResourceDefinitionAccessor();
        var sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        var requestQueryStringAccessor = new FakeRequestQueryStringAccessor();

        ResponseModelAdapter = new ResponseModelAdapter(request, options, linkBuilder, metaBuilder, resourceDefinitionAccessor, evaluatedIncludeCache,
            sparseFieldSetCache, requestQueryStringAccessor);
    }

    protected abstract JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph);

    protected abstract IEvaluatedIncludeCache CreateEvaluatedIncludeCache(IResourceGraph resourceGraph);

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OutgoingResource : Identifiable<int>
    {
        [Attr]
        public bool Attribute01 { get; set; }

        [Attr]
        public char Attribute02 { get; set; }

        [Attr]
        public ulong? Attribute03 { get; set; }

        [Attr]
        public decimal Attribute04 { get; set; }

        [Attr]
        public float? Attribute05 { get; set; }

        [Attr]
        public string Attribute06 { get; set; } = null!;

        [Attr]
        public DateTime? Attribute07 { get; set; }

        [Attr]
        public DateTimeOffset? Attribute08 { get; set; }

        [Attr]
        public TimeSpan? Attribute09 { get; set; }

        [Attr]
        public DayOfWeek Attribute10 { get; set; }

        [HasOne]
        public OutgoingResource Single1 { get; set; } = null!;

        [HasOne]
        public OutgoingResource Single2 { get; set; } = null!;

        [HasOne]
        public OutgoingResource Single3 { get; set; } = null!;

        [HasOne]
        public OutgoingResource Single4 { get; set; } = null!;

        [HasOne]
        public OutgoingResource Single5 { get; set; } = null!;

        [HasMany]
        public ISet<OutgoingResource> Multi1 { get; set; } = null!;

        [HasMany]
        public ISet<OutgoingResource> Multi2 { get; set; } = null!;

        [HasMany]
        public ISet<OutgoingResource> Multi3 { get; set; } = null!;

        [HasMany]
        public ISet<OutgoingResource> Multi4 { get; set; } = null!;

        [HasMany]
        public ISet<OutgoingResource> Multi5 { get; set; } = null!;
    }

    private sealed class FakeResourceDefinitionAccessor : IResourceDefinitionAccessor
    {
        public IImmutableSet<IncludeElementExpression> OnApplyIncludes(ResourceType resourceType, IImmutableSet<IncludeElementExpression> existingIncludes)
        {
            return existingIncludes;
        }

        public FilterExpression? OnApplyFilter(ResourceType resourceType, FilterExpression? existingFilter)
        {
            return existingFilter;
        }

        public SortExpression? OnApplySort(ResourceType resourceType, SortExpression? existingSort)
        {
            return existingSort;
        }

        public PaginationExpression? OnApplyPagination(ResourceType resourceType, PaginationExpression? existingPagination)
        {
            return existingPagination;
        }

        public SparseFieldSetExpression? OnApplySparseFieldSet(ResourceType resourceType, SparseFieldSetExpression? existingSparseFieldSet)
        {
            return existingSparseFieldSet;
        }

        public object? GetQueryableHandlerForQueryStringParameter(Type resourceClrType, string parameterName)
        {
            return null;
        }

        public IDictionary<string, object?>? GetMeta(ResourceType resourceType, IIdentifiable resourceInstance)
        {
            return null;
        }

        public Task OnPrepareWriteAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task<IIdentifiable?> OnSetToOneRelationshipAsync<TResource>(TResource leftResource, HasOneAttribute hasOneRelationship,
            IIdentifiable? rightResourceId, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.FromResult(rightResourceId);
        }

        public Task OnSetToManyRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnAddToRelationshipAsync<TResource, TId>(TId leftResourceId, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>
        {
            return Task.CompletedTask;
        }

        public Task OnRemoveFromRelationshipAsync<TResource>(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnWritingAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public Task OnWriteSucceededAsync<TResource>(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable
        {
            return Task.CompletedTask;
        }

        public void OnDeserialize(IIdentifiable resource)
        {
        }

        public void OnSerialize(IIdentifiable resource)
        {
        }
    }

    private sealed class FakeLinkBuilder : ILinkBuilder
    {
        public TopLevelLinks GetTopLevelLinks()
        {
            return new TopLevelLinks
            {
                Self = "TopLevel:Self"
            };
        }

        public ResourceLinks GetResourceLinks(ResourceType resourceType, IIdentifiable resource)
        {
            return new ResourceLinks
            {
                Self = "Resource:Self"
            };
        }

        public RelationshipLinks GetRelationshipLinks(RelationshipAttribute relationship, IIdentifiable leftResource)
        {
            return new RelationshipLinks
            {
                Self = "Relationship:Self",
                Related = "Relationship:Related"
            };
        }
    }

    private sealed class FakeMetaBuilder : IMetaBuilder
    {
        public void Add(IReadOnlyDictionary<string, object?> values)
        {
        }

        public IDictionary<string, object?>? Build()
        {
            return null;
        }
    }

    private sealed class FakeRequestQueryStringAccessor : IRequestQueryStringAccessor
    {
        public IQueryCollection Query { get; } = new QueryCollection(0);
    }
}
