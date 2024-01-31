using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    public static void AddOpenApi(this IServiceCollection services, IMvcCoreBuilder mvcBuilder, Action<SwaggerGenOptions>? setupSwaggerGenAction = null)
    {
        ArgumentGuard.NotNull(services);
        ArgumentGuard.NotNull(mvcBuilder);

        AddCustomApiExplorer(services, mvcBuilder);
        AddCustomSwaggerComponents(services);
        AddSwaggerGenerator(services);

        services.AddTransient<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();

        if (setupSwaggerGenAction != null)
        {
            services.Configure(setupSwaggerGenAction);
        }
    }

    private static void AddCustomApiExplorer(IServiceCollection services, IMvcCoreBuilder mvcBuilder)
    {
        services.TryAddSingleton<ResourceFieldValidationMetadataProvider>();

        services.TryAddSingleton<IApiDescriptionGroupCollectionProvider>(provider =>
        {
            var controllerResourceMapping = provider.GetRequiredService<IControllerResourceMapping>();
            var actionDescriptorCollectionProvider = provider.GetRequiredService<IActionDescriptorCollectionProvider>();
            var apiDescriptionProviders = provider.GetRequiredService<IEnumerable<IApiDescriptionProvider>>();
            var resourceFieldValidationMetadataProvider = provider.GetRequiredService<ResourceFieldValidationMetadataProvider>();

            JsonApiActionDescriptorCollectionProvider jsonApiActionDescriptorCollectionProvider =
                new(controllerResourceMapping, actionDescriptorCollectionProvider, resourceFieldValidationMetadataProvider);

            return new ApiDescriptionGroupCollectionProvider(jsonApiActionDescriptorCollectionProvider, apiDescriptionProviders);
        });

        mvcBuilder.AddApiExplorer();

        mvcBuilder.AddMvcOptions(options => options.InputFormatters.Add(new JsonApiRequestFormatMetadataProvider()));
    }

    private static void AddCustomSwaggerComponents(IServiceCollection services)
    {
        services.TryAddSingleton<ISerializerDataContractResolver, JsonApiDataContractResolver>();
        services.TryAddSingleton<ResourceDocumentationReader>();
        services.TryAddSingleton<JsonApiOperationIdSelector>();
        services.TryAddSingleton<JsonApiSchemaIdSelector>();
    }

    private static void AddSwaggerGenerator(IServiceCollection services)
    {
        AddSchemaGenerators(services);

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
        services.TryAddSingleton<ResourceDataSchemaGenerator>();
    }
}
