using System.ComponentModel.Design;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization;

/// <summary>
/// Builds the request adapter pipeline on top of a real resource graph, so that JSON request bodies can be converted without an HTTP host.
/// </summary>
internal sealed class RequestAdapterHarness : IDisposable
{
    private readonly ServiceContainer _serviceProvider = new();
    private readonly JsonApiOptions _options = new();
    private readonly IResourceGraph _resourceGraph;
    private readonly IDocumentInResourceOrRelationshipRequestAdapter _resourceDocumentAdapter;
    private readonly IDocumentInOperationsRequestAdapter _operationsDocumentAdapter;

    public RequestAdapterHarness(Action<JsonApiOptions>? configureOptions, params Type[] resourceClrTypes)
    {
        configureOptions?.Invoke(_options);

        var builder = new ResourceGraphBuilder(_options, NullLoggerFactory.Instance);

        foreach (Type resourceClrType in resourceClrTypes)
        {
            builder.Add(resourceClrType, typeof(long));
        }

        _resourceGraph = builder.Build();
        _options.SerializerOptions.Converters.Add(new ResourceObjectConverter(_resourceGraph));

        var resourceFactory = new ResourceFactory(_serviceProvider);
        var resourceDefinitionAccessor = new ResourceDefinitionAccessor(_resourceGraph, _serviceProvider);
        _serviceProvider.AddService(typeof(IResourceDefinitionAccessor), resourceDefinitionAccessor);

        foreach (Type resourceClrType in resourceClrTypes)
        {
            Type serviceType = typeof(IResourceDefinition<,>).MakeGenericType(resourceClrType, typeof(long));
            Type implementationType = typeof(JsonApiResourceDefinition<,>).MakeGenericType(resourceClrType, typeof(long));
            _serviceProvider.AddService(serviceType, Activator.CreateInstance(implementationType, _resourceGraph)!);
        }

        var resourceIdentifierObjectAdapter = new ResourceIdentifierObjectAdapter(_resourceGraph, resourceFactory);
        var relationshipDataAdapter = new RelationshipDataAdapter(resourceIdentifierObjectAdapter);
        var resourceObjectAdapter = new ResourceObjectAdapter(_resourceGraph, resourceFactory, _options, relationshipDataAdapter);
        var resourceDataAdapter = new ResourceDataAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

        var atomicReferenceAdapter = new AtomicReferenceAdapter(_resourceGraph, resourceFactory);
        var operationResourceDataAdapter = new ResourceDataInOperationsRequestAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

        var atomicOperationObjectAdapter = new AtomicOperationObjectAdapter(_options, atomicReferenceAdapter, operationResourceDataAdapter,
            relationshipDataAdapter);

        _resourceDocumentAdapter = new DocumentInResourceOrRelationshipRequestAdapter(_options, resourceDataAdapter, relationshipDataAdapter);
        _operationsDocumentAdapter = new DocumentInOperationsRequestAdapter(_options, atomicOperationObjectAdapter);
    }

    /// <summary>
    /// Creates a request body with a single resource object, from the specified JSON for the "attributes" object.
    /// </summary>
    public string CreateResourceDocument(Type resourceClrType, string attributesJson, bool includeId)
    {
        return $$"""{"data":{{CreateResourceObject(resourceClrType, attributesJson, includeId)}}}""";
    }

    /// <summary>
    /// Creates an atomic:operations request body with a single "add" operation, from the specified JSON for the "attributes" object.
    /// </summary>
    public string CreateOperationsDocument(Type resourceClrType, string attributesJson)
    {
        return $$"""{"atomic:operations":[{"op":"add","data":{{CreateResourceObject(resourceClrType, attributesJson, false)}}}]}""";
    }

    private string CreateResourceObject(Type resourceClrType, string attributesJson, bool includeId)
    {
        string publicName = _resourceGraph.GetResourceType(resourceClrType).PublicName;
        string idPart = includeId ? "\"id\":\"1\"," : string.Empty;

        return $$"""{"type":"{{publicName}}",{{idPart}}"attributes":{{attributesJson}}}""";
    }

    public TResource ConvertResource<TResource>(string documentJson, WriteOperationKind writeOperation)
        where TResource : class, IIdentifiable
    {
        var document = JsonSerializer.Deserialize<Document>(documentJson, ((IJsonApiOptions)_options).SerializerReadOptions)!;

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = _resourceGraph.GetResourceType<TResource>(),
            PrimaryId = writeOperation == WriteOperationKind.UpdateResource ? "1" : null,
            WriteOperation = writeOperation
        };

        var targetedFields = new TargetedFields();
        var documentAdapter = new DocumentAdapter(request, targetedFields, _resourceDocumentAdapter, _operationsDocumentAdapter);

        return (TResource)documentAdapter.Convert(document)!;
    }

    public IList<OperationContainer> ConvertOperations(string documentJson)
    {
        var document = JsonSerializer.Deserialize<Document>(documentJson, ((IJsonApiOptions)_options).SerializerReadOptions)!;

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.AtomicOperations
        };

        var targetedFields = new TargetedFields();
        var documentAdapter = new DocumentAdapter(request, targetedFields, _resourceDocumentAdapter, _operationsDocumentAdapter);

        return (IList<OperationContainer>)documentAdapter.Convert(document)!;
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}
