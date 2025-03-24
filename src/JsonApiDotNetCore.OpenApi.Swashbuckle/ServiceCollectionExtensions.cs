using System.Diagnostics.CodeAnalysis;
using JsonApiDotNetCore.Configuration;
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
    [Experimental("JADNC_OA_001", UrlFormat = "https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/openapi/docs/usage/openapi.md")]
    public static void AddOpenApiForJsonApi(this IServiceCollection services, Action<SwaggerGenOptions>? configureSwaggerGenOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddCustomApiExplorer(services);
        AddCustomSwaggerComponents(services);
        AddSwaggerGenerator(services);

        if (configureSwaggerGenOptions != null)
        {
            services.Configure(configureSwaggerGenOptions);
        }

        services.AddSingleton<IJsonApiContentNegotiator, OpenApiContentNegotiator>();
        services.TryAddSingleton<IJsonApiRequestAccessor, JsonApiRequestAccessor>();
        services.Replace(ServiceDescriptor.Singleton<IJsonApiApplicationBuilderEvents, OpenApiApplicationBuilderEvents>());
    }

    private static void AddCustomApiExplorer(IServiceCollection services)
    {
        services.TryAddSingleton<OpenApiEndpointConvention>();
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
        // The code below was copied from the implementation of MvcApiExplorerMvcCoreBuilderExtensions.AddApiExplorer(),
        // so we don't need to take IMvcCoreBuilder as an input parameter.

        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());
    }

    private static void AddCustomSwaggerComponents(IServiceCollection services)
    {
        services.TryAddSingleton<ISerializerDataContractResolver, JsonApiDataContractResolver>();
        services.TryAddSingleton<ResourceDocumentationReader>();
        services.TryAddSingleton<OpenApiOperationIdSelector>();
        services.TryAddSingleton<JsonApiSchemaIdSelector>();
    }

    private static void AddSwaggerGenerator(IServiceCollection services)
    {
        AddSchemaGenerators(services);

        services.TryAddSingleton<RelationshipTypeFactory>();
        services.AddSingleton<IDocumentDescriptionLinkProvider, OpenApiDescriptionLinkProvider>();

        services.AddSwaggerGen();
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
    }
}
