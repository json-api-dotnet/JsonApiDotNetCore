using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using SchemaGenerator = Swashbuckle.AspNetCore.SwaggerGen.Patched.SchemaGenerator;

namespace JsonApiDotNetCore.OpenApi;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the OpenAPI integration to JsonApiDotNetCore by configuring Swashbuckle.
    /// </summary>
    public static void AddOpenApi(this IServiceCollection services, IMvcCoreBuilder mvcBuilder, Action<SwaggerGenOptions>? setupSwaggerGenAction = null)
    {
        ArgumentGuard.NotNull(services);
        ArgumentGuard.NotNull(mvcBuilder);

        AddCustomApiExplorer(services, mvcBuilder);
        AddCustomSwaggerComponents(services);
        AddSwaggerGenerator(services);

        if (setupSwaggerGenAction != null)
        {
            services.Configure(setupSwaggerGenAction);
        }
    }

    private static void AddCustomApiExplorer(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
    {
        services.TryAddSingleton<OpenApiEndpointConvention>();
        services.TryAddSingleton<JsonApiRequestFormatMetadataProvider>();
        services.TryAddSingleton<EndpointResolver>();
        services.TryAddSingleton<JsonApiEndpointMetadataProvider>();
        services.TryAddSingleton<JsonApiActionDescriptorCollectionProvider>();
        services.TryAddSingleton<NonPrimaryDocumentTypeFactory>();
        services.TryAddSingleton<ResourceFieldValidationMetadataProvider>();

        services.TryAddSingleton<IApiDescriptionGroupCollectionProvider>(serviceProvider =>
        {
            var actionDescriptorCollectionProvider = serviceProvider.GetRequiredService<JsonApiActionDescriptorCollectionProvider>();
            var apiDescriptionProviders = serviceProvider.GetRequiredService<IEnumerable<IApiDescriptionProvider>>();

            return new ApiDescriptionGroupCollectionProvider(actionDescriptorCollectionProvider, apiDescriptionProviders);
        });

        mvcBuilder.AddApiExplorer();

        services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
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

        services.AddSwaggerGen();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
    }

    private static void AddSchemaGenerators(IServiceCollection services)
    {
        services.TryAddSingleton<SchemaGenerator>();
        services.TryAddSingleton<ISchemaGenerator, JsonApiSchemaGenerator>();

        services.TryAddSingleton<DocumentSchemaGenerator>();
        services.TryAddSingleton<ResourceTypeSchemaGenerator>();
        services.TryAddSingleton<ResourceIdentifierSchemaGenerator>();
        services.TryAddSingleton<AbstractResourceDataSchemaGenerator>();
        services.TryAddSingleton<ResourceDataSchemaGenerator>();
    }
}
