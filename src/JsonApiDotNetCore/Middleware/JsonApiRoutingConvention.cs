using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// The default routing convention registers the name of the resource as the route using the serializer naming convention. The default for this is a
/// camel case formatter. If the controller directly inherits from <see cref="CoreJsonApiController" /> and there is no resource directly associated, it
/// uses the name of the controller instead of the name of the type.
/// </summary>
/// <example><![CDATA[
/// public class SomeResourceController : JsonApiController<SomeResource> { } // => /someResources/relationship/relatedResource
/// 
/// public class RandomNameController<SomeResource> : JsonApiController<SomeResource> { } // => /someResources/relationship/relatedResource
/// 
/// // when using kebab-case naming convention:
/// public class SomeResourceController<SomeResource> : JsonApiController<SomeResource> { } // => /some-resources/relationship/related-resource
/// 
/// public class SomeVeryCustomController<SomeResource> : CoreJsonApiController { } // => /someVeryCustoms/relationship/relatedResource
/// ]]></example>
[PublicAPI]
public sealed class JsonApiRoutingConvention : IJsonApiRoutingConvention
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly ILogger<JsonApiRoutingConvention> _logger;
    private readonly Dictionary<string, string> _registeredControllerNameByTemplate = new();
    private readonly Dictionary<Type, ResourceType> _resourceTypePerControllerTypeMap = new();
    private readonly Dictionary<ResourceType, ControllerModel> _controllerPerResourceTypeMap = new();

    public JsonApiRoutingConvention(IJsonApiOptions options, IResourceGraph resourceGraph, ILogger<JsonApiRoutingConvention> logger)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(logger);

        _options = options;
        _resourceGraph = resourceGraph;
        _logger = logger;
    }

    /// <inheritdoc />
    public ResourceType? GetResourceTypeForController(Type? controllerType)
    {
        return controllerType != null && _resourceTypePerControllerTypeMap.TryGetValue(controllerType, out ResourceType? resourceType) ? resourceType : null;
    }

    /// <inheritdoc />
    public string? GetControllerNameForResourceType(ResourceType? resourceType)
    {
        return resourceType != null && _controllerPerResourceTypeMap.TryGetValue(resourceType, out ControllerModel? controllerModel)
            ? controllerModel.ControllerName
            : null;
    }

    /// <inheritdoc />
    public void Apply(ApplicationModel application)
    {
        ArgumentGuard.NotNull(application);

        foreach (ControllerModel controller in application.Controllers)
        {
            if (!IsJsonApiController(controller))
            {
                continue;
            }

            if (HasApiControllerAttribute(controller))
            {
                // Although recommended by Microsoft for hard-written controllers, the opinionated behavior of [ApiController] violates the JSON:API specification.
                // See https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-7.0#apicontroller-attribute for its effects.
                // JsonApiDotNetCore already handles all of these concerns, but in a JSON:API-compliant way. So the attribute doesn't do any good.

                // While we try our best when [ApiController] is used, we can't completely avoid a degraded experience. ModelState validation errors are turned into
                // ProblemDetails, where the origin of the error gets lost. As a result, we can't populate the source pointer in JSON:API error responses.
                // For backwards-compatibility, we log a warning instead of throwing. But we can't think of any use cases where having [ApiController] makes sense.

                _logger.LogWarning(
                    $"Found JSON:API controller '{controller.ControllerType}' with [ApiController]. Please remove this attribute for optimal JSON:API compliance.");
            }

            if (!IsOperationsController(controller.ControllerType))
            {
                Type? resourceClrType = ExtractResourceClrTypeFromController(controller.ControllerType);

                if (resourceClrType != null)
                {
                    ResourceType? resourceType = _resourceGraph.FindResourceType(resourceClrType);

                    if (resourceType == null)
                    {
                        throw new InvalidConfigurationException($"Controller '{controller.ControllerType}' depends on " +
                            $"resource type '{resourceClrType}', which does not exist in the resource graph.");
                    }

                    if (_controllerPerResourceTypeMap.ContainsKey(resourceType))
                    {
                        throw new InvalidConfigurationException(
                            $"Multiple controllers found for resource type '{resourceType}': '{_controllerPerResourceTypeMap[resourceType].ControllerType}' and '{controller.ControllerType}'.");
                    }

                    _resourceTypePerControllerTypeMap.Add(controller.ControllerType, resourceType);
                    _controllerPerResourceTypeMap.Add(resourceType, controller);
                }
            }

            if (IsRoutingConventionDisabled(controller))
            {
                continue;
            }

            string template = TemplateFromResource(controller) ?? TemplateFromController(controller);

            if (_registeredControllerNameByTemplate.ContainsKey(template))
            {
                throw new InvalidConfigurationException(
                    $"Cannot register '{controller.ControllerType.FullName}' for template '{template}' because '{_registeredControllerNameByTemplate[template]}' was already registered for this template.");
            }

            _registeredControllerNameByTemplate.Add(template, controller.ControllerType.FullName!);

            controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel
            {
                Template = template
            };
        }
    }

    private static bool IsJsonApiController(ControllerModel controller)
    {
        return controller.ControllerType.IsSubclassOf(typeof(CoreJsonApiController));
    }

    private static bool HasApiControllerAttribute(ControllerModel controller)
    {
        return controller.ControllerType.GetCustomAttribute<ApiControllerAttribute>() != null;
    }

    private static bool IsRoutingConventionDisabled(ControllerModel controller)
    {
        return controller.ControllerType.GetCustomAttribute<DisableRoutingConventionAttribute>(true) != null;
    }

    /// <summary>
    /// Derives a template from the resource type, and checks if this template was already registered.
    /// </summary>
    private string? TemplateFromResource(ControllerModel model)
    {
        if (_resourceTypePerControllerTypeMap.TryGetValue(model.ControllerType, out ResourceType? resourceType))
        {
            return $"{_options.Namespace}/{resourceType.PublicName}";
        }

        return null;
    }

    /// <summary>
    /// Derives a template from the controller name, and checks if this template was already registered.
    /// </summary>
    private string TemplateFromController(ControllerModel model)
    {
        string controllerName = _options.SerializerOptions.PropertyNamingPolicy == null
            ? model.ControllerName
            : _options.SerializerOptions.PropertyNamingPolicy.ConvertName(model.ControllerName);

        return $"{_options.Namespace}/{controllerName}";
    }

    /// <summary>
    /// Determines the resource type associated to a controller by inspecting generic type arguments in its inheritance tree.
    /// </summary>
    private Type? ExtractResourceClrTypeFromController(Type controllerType)
    {
        Type aspNetControllerType = typeof(ControllerBase);
        Type coreControllerType = typeof(CoreJsonApiController);
        Type baseControllerUnboundType = typeof(BaseJsonApiController<,>);
        Type? currentType = controllerType;

        while (!currentType.IsGenericType || currentType.GetGenericTypeDefinition() != baseControllerUnboundType)
        {
            Type? nextBaseType = currentType.BaseType;

            if ((nextBaseType == aspNetControllerType || nextBaseType == coreControllerType) && currentType.IsGenericType)
            {
                Type? resourceClrType = currentType.GetGenericArguments().FirstOrDefault(typeArgument => typeArgument.IsOrImplementsInterface<IIdentifiable>());

                if (resourceClrType != null)
                {
                    return resourceClrType;
                }
            }

            currentType = nextBaseType;

            if (currentType == null)
            {
                break;
            }
        }

        return currentType?.GetGenericArguments().First();
    }

    private static bool IsOperationsController(Type type)
    {
        Type baseControllerType = typeof(BaseJsonApiOperationsController);
        return baseControllerType.IsAssignableFrom(type);
    }
}
