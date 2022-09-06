using System.Text.Json;
using System.Text.Json.Serialization;
using Benchmarks.Tools;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Response;
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
        var metaBuilder = new NoMetaBuilder();
        IQueryConstraintProvider[] constraintProviders = Array.Empty<IQueryConstraintProvider>();
        var resourceDefinitionAccessor = new NeverResourceDefinitionAccessor();
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
}
