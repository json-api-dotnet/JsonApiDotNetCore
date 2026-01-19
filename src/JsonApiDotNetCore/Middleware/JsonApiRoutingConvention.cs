using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Registers routes based on the JSON:API resource name, which defaults to camel-case pluralized form of the resource CLR type name. If unavailable (for
/// example, when a controller directly inherits from <see cref="CoreJsonApiController" />), the serializer naming convention is applied on the
/// controller type name (camel-case by default).
/// </summary>
/// <example>
/// <code><![CDATA[
/// // controller name is ignored when resource type is available:
/// public class RandomNameController<SomeResource> : JsonApiController<SomeResource> { } // => /someResources
/// 
/// // when using kebab-case naming convention in options:
/// public class RandomNameController<SomeResource> : JsonApiController<SomeResource> { } // => /some-resources
/// 
/// // unable to determine resource type:
/// public class SomeVeryCustomController<SomeResource> : CoreJsonApiController { } // => /someVeryCustom
/// ]]></code>
/// </example>
[PublicAPI]
public sealed partial class JsonApiRoutingConvention : IJsonApiRoutingConvention
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiEndpointFilter _jsonApiEndpointFilter;
    private readonly ILogger<JsonApiRoutingConvention> _logger;
    private readonly Dictionary<string, string> _registeredControllerNameByTemplate = [];
    private readonly Dictionary<Type, ResourceType> _resourceTypePerControllerTypeMap = [];
    private readonly Dictionary<ResourceType, ControllerModel> _controllerPerResourceTypeMap = [];

    public JsonApiRoutingConvention(IJsonApiOptions options, IResourceGraph resourceGraph, IJsonApiEndpointFilter jsonApiEndpointFilter,
        ILogger<JsonApiRoutingConvention> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(jsonApiEndpointFilter);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _resourceGraph = resourceGraph;
        _jsonApiEndpointFilter = jsonApiEndpointFilter;
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
        ArgumentNullException.ThrowIfNull(application);

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

                LogApiControllerAttributeFound(controller.ControllerType);
            }

            if (!IsOperationsController(controller.ControllerType))
            {
                Type? resourceClrType = ExtractResourceClrTypeFromController(controller.ControllerType);

                if (resourceClrType != null)
                {
                    ResourceType? resourceType = _resourceGraph.FindResourceType(resourceClrType);

                    if (resourceType == null)
                    {
                        throw new InvalidConfigurationException(
                            $"Controller '{controller.ControllerType}' depends on resource type '{resourceClrType}', which does not exist in the resource graph.");
                    }

                    if (_controllerPerResourceTypeMap.TryGetValue(resourceType, out ControllerModel? existingModel))
                    {
                        throw new InvalidConfigurationException(
                            $"Multiple controllers found for resource type '{resourceType}': '{existingModel.ControllerType}' and '{controller.ControllerType}'.");
                    }

                    RemoveDisabledActionMethods(controller, resourceType);

                    _resourceTypePerControllerTypeMap.Add(controller.ControllerType, resourceType);
                    _controllerPerResourceTypeMap.Add(resourceType, controller);
                }
            }
            else
            {
                var options = (JsonApiOptions)_options;
                options.IncludeExtensions(JsonApiMediaTypeExtension.AtomicOperations, JsonApiMediaTypeExtension.RelaxedAtomicOperations);
            }

            if (IsRoutingConventionDisabled(controller))
            {
                continue;
            }

            string template = TemplateFromResource(controller) ?? TemplateFromController(controller);

            if (_registeredControllerNameByTemplate.TryGetValue(template, out string? controllerName))
            {
                throw new InvalidConfigurationException(
                    $"Cannot register '{controller.ControllerType.FullName}' for template '{template}' because '{controllerName}' was already registered for this template.");
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

    private static bool IsOperationsController(Type type)
    {
        Type baseControllerType = typeof(BaseJsonApiOperationsController);
        return baseControllerType.IsAssignableFrom(type);
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

    private void RemoveDisabledActionMethods(ControllerModel controller, ResourceType resourceType)
    {
        foreach (ActionModel actionModel in controller.Actions.ToArray())
        {
            JsonApiEndpoints endpoint = actionModel.Attributes.OfType<HttpMethodAttribute>().GetJsonApiEndpoint();

            if (endpoint != JsonApiEndpoints.None && !_jsonApiEndpointFilter.IsEnabled(resourceType, endpoint))
            {
                controller.Actions.Remove(actionModel);
            }
        }
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

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Found JSON:API controller '{ControllerType}' with [ApiController]. Please remove this attribute for optimal JSON:API compliance.")]
    private partial void LogApiControllerAttributeFound(TypeInfo controllerType);
}
