using System.ComponentModel.Design;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.Deserialization;

public abstract class DeserializationBenchmarkBase : IDisposable
{
    private readonly ServiceContainer _serviceProvider = new();

    protected JsonSerializerOptions SerializerReadOptions { get; }
    protected DocumentAdapter DocumentAdapter { get; }

    protected DeserializationBenchmarkBase()
    {
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<IncomingResource, int>().Build();
        options.SerializerOptions.Converters.Add(new ResourceObjectConverter(resourceGraph));
        SerializerReadOptions = ((IJsonApiOptions)options).SerializerReadOptions;

        var resourceFactory = new ResourceFactory(_serviceProvider);
        var resourceDefinitionAccessor = new ResourceDefinitionAccessor(resourceGraph, _serviceProvider);

        _serviceProvider.AddService(typeof(IResourceDefinitionAccessor), resourceDefinitionAccessor);
        _serviceProvider.AddService(typeof(IResourceDefinition<IncomingResource, int>), new JsonApiResourceDefinition<IncomingResource, int>(resourceGraph));

        // ReSharper disable once VirtualMemberCallInConstructor
        JsonApiRequest request = CreateJsonApiRequest(resourceGraph);
        var targetedFields = new TargetedFields();

        var resourceIdentifierObjectAdapter = new ResourceIdentifierObjectAdapter(resourceGraph, resourceFactory);
        var relationshipDataAdapter = new RelationshipDataAdapter(resourceIdentifierObjectAdapter);
        var resourceObjectAdapter = new ResourceObjectAdapter(resourceGraph, resourceFactory, options, relationshipDataAdapter);
        var resourceDataAdapter = new ResourceDataAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

        var atomicReferenceAdapter = new AtomicReferenceAdapter(resourceGraph, resourceFactory);
        var atomicOperationResourceDataAdapter = new ResourceDataInOperationsRequestAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

        var atomicOperationObjectAdapter = new AtomicOperationObjectAdapter(options, atomicReferenceAdapter,
            atomicOperationResourceDataAdapter, relationshipDataAdapter);

        var resourceDocumentAdapter = new DocumentInResourceOrRelationshipRequestAdapter(options, resourceDataAdapter, relationshipDataAdapter);
        var operationsDocumentAdapter = new DocumentInOperationsRequestAdapter(options, atomicOperationObjectAdapter);

        DocumentAdapter = new DocumentAdapter(request, targetedFields, resourceDocumentAdapter, operationsDocumentAdapter);
    }

    protected abstract JsonApiRequest CreateJsonApiRequest(IResourceGraph resourceGraph);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

#pragma warning disable CA1063 // Implement IDisposable Correctly
    private void Dispose(bool disposing)
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        if (disposing)
        {
            _serviceProvider.Dispose();
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IncomingResource : Identifiable<int>
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
        public IncomingResource Single1 { get; set; } = null!;

        [HasOne]
        public IncomingResource Single2 { get; set; } = null!;

        [HasOne]
        public IncomingResource Single3 { get; set; } = null!;

        [HasOne]
        public IncomingResource Single4 { get; set; } = null!;

        [HasOne]
        public IncomingResource Single5 { get; set; } = null!;

        [HasMany]
        public ISet<IncomingResource> Multi1 { get; set; } = null!;

        [HasMany]
        public ISet<IncomingResource> Multi2 { get; set; } = null!;

        [HasMany]
        public ISet<IncomingResource> Multi3 { get; set; } = null!;

        [HasMany]
        public ISet<IncomingResource> Multi4 { get; set; } = null!;

        [HasMany]
        public ISet<IncomingResource> Multi5 { get; set; } = null!;
    }
}
