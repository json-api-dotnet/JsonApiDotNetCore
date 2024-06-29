using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.SchemaGenerators;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Bodies;
using JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the OpenAPI integration to JsonApiDotNetCore by configuring Swashbuckle.
    /// </summary>
    public static void AddOpenApi(this IServiceCollection services, Action<SwaggerGenOptions>? setupSwaggerGenAction = null)
    {
        ArgumentGuard.NotNull(services);

        AddCustomApiExplorer(services);
        AddCustomSwaggerComponents(services);
        AddSwaggerGenerator(services);

        if (setupSwaggerGenAction != null)
        {
            services.Configure(setupSwaggerGenAction);
        }
    }

    private static void AddCustomApiExplorer(IServiceCollection services)
    {
        services.TryAddSingleton<OpenApiEndpointConvention>();
        services.TryAddSingleton<JsonApiRequestFormatMetadataProvider>();
        services.TryAddSingleton<EndpointResolver>();
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
        services.TryAddSingleton<JsonApiOperationIdSelector>();
        services.TryAddSingleton<JsonApiSchemaIdSelector>();
        services.TryAddSingleton<IncludeDependencyScanner>();
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

        services.TryAddEnumerable(ServiceDescriptor.Singleton<BodySchemaGenerator, ResourceOrRelationshipBodySchemaGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<BodySchemaGenerator, ErrorResponseBodySchemaGenerator>());

        services.TryAddSingleton<ResourceTypeSchemaGenerator>();
        services.TryAddSingleton<MetaSchemaGenerator>();
        services.TryAddSingleton<ResourceIdentifierSchemaGenerator>();
        services.TryAddSingleton<AbstractResourceDataSchemaGenerator>();
        services.TryAddSingleton<DataSchemaGenerator>();
        services.TryAddSingleton<LinksVisibilitySchemaGenerator>();
    }
}
