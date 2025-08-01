using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures OpenAPI for JsonApiDotNetCore using Swashbuckle.
    /// </summary>
    public static void AddOpenApiForJsonApi(this IServiceCollection services, Action<SwaggerGenOptions>? configureSwaggerGenOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        AssertHasJsonApi(services);

        AddCustomApiExplorer(services);
        AddCustomSwaggerComponents(services);
        AddSwaggerGenerator(services, configureSwaggerGenOptions);

        services.Replace(ServiceDescriptor.Singleton<IJsonApiContentNegotiator, OpenApiContentNegotiator>());
        services.TryAddSingleton<IJsonApiRequestAccessor, JsonApiRequestAccessor>();
        services.Replace(ServiceDescriptor.Singleton<IJsonApiApplicationBuilderEvents, OpenApiApplicationBuilderEvents>());
    }

    private static void AssertHasJsonApi(IServiceCollection services)
    {
        if (services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IJsonApiOptions)) == null)
        {
            throw new InvalidConfigurationException("Call 'services.AddJsonApi()' before calling 'services.AddOpenApiForJsonApi()'.");
        }
    }

    private static void AddCustomApiExplorer(IServiceCollection services)
    {
        services.TryAddSingleton<JsonApiRequestFormatMetadataProvider>();
        services.TryAddSingleton<JsonApiEndpointMetadataProvider>();
        services.TryAddSingleton<JsonApiActionDescriptorCollectionProvider>();
        services.TryAddSingleton<NonPrimaryDocumentTypeFactory>();
        services.TryAddSingleton<ResourceFieldValidationMetadataProvider>();

        // Not using TryAddSingleton, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1463.
        services.Replace(ServiceDescriptor.Singleton<IApiDescriptionGroupCollectionProvider>(serviceProvider =>
        {
            var actionDescriptorCollectionProvider = serviceProvider.GetRequiredService<JsonApiActionDescriptorCollectionProvider>();
            var apiDescriptionProviders = serviceProvider.GetRequiredService<IEnumerable<IApiDescriptionProvider>>();

            return new ApiDescriptionGroupCollectionProvider(actionDescriptorCollectionProvider, apiDescriptionProviders);
        }));

        AddApiExplorer(services);

        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
    }

    private static void AddApiExplorer(IServiceCollection services)
    {
        // This call was copied from the implementation of MvcApiExplorerMvcCoreBuilderExtensions.AddApiExplorer(),
        // so we don't need to take IMvcCoreBuilder as an input parameter.
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());

        // This call ensures that Minimal API endpoints appear in Swashbuckle.
        // Don't be fooled to believe this call is redundant: When running from Visual Studio, a startup filter is injected
        // that also calls this. But that doesn't happen when running from the command line or from an integration test.
        services.AddEndpointsApiExplorer();
    }

    private static void AddCustomSwaggerComponents(IServiceCollection services)
    {
        services.TryAddSingleton<ISerializerDataContractResolver, JsonApiDataContractResolver>();
        services.TryAddSingleton<ResourceDocumentationReader>();
        services.TryAddSingleton<OpenApiOperationIdSelector>();
        services.TryAddSingleton<JsonApiSchemaIdSelector>();
    }

    private static void AddSwaggerGenerator(IServiceCollection services, Action<SwaggerGenOptions>? configureSwaggerGenOptions)
    {
        AddSchemaGenerators(services);

        services.TryAddSingleton<RelationshipTypeFactory>();
        services.Replace(ServiceDescriptor.Singleton<IDocumentDescriptionLinkProvider, OpenApiDescriptionLinkProvider>());

        services.AddSwaggerGen(configureSwaggerGenOptions);
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
    }

    private static void AddSchemaGenerators(IServiceCollection services)
    {
        services.TryAddSingleton<SchemaGenerator>();
        services.TryAddSingleton<ISchemaGenerator, JsonApiSchemaGenerator>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<DocumentSchemaGenerator, ResourceOrRelationshipDocumentSchemaGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<DocumentSchemaGenerator, AtomicOperationsDocumentSchemaGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<DocumentSchemaGenerator, ErrorResponseDocumentSchemaGenerator>());

        services.TryAddSingleton<AtomicOperationCodeSchemaGenerator>();
        services.TryAddSingleton<ResourceTypeSchemaGenerator>();
        services.TryAddSingleton<ResourceIdSchemaGenerator>();
        services.TryAddSingleton<MetaSchemaGenerator>();
        services.TryAddSingleton<RelationshipIdentifierSchemaGenerator>();
        services.TryAddSingleton<RelationshipNameSchemaGenerator>();
        services.TryAddSingleton<DataSchemaGenerator>();
        services.TryAddSingleton<DataContainerSchemaGenerator>();
        services.TryAddSingleton<LinksVisibilitySchemaGenerator>();
        services.TryAddSingleton<GenerationCacheSchemaGenerator>();

        services.TryAddSingleton<SchemaGenerationTracer>();
    }
}
