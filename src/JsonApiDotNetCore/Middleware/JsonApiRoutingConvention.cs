using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

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
    private readonly Dictionary<string, string> _registeredControllerNameByTemplate = new();
    private readonly Dictionary<Type, ResourceType> _resourceTypePerControllerTypeMap = new();
    private readonly Dictionary<ResourceType, ControllerModel> _controllerPerResourceTypeMap = new();

    public JsonApiRoutingConvention(IJsonApiOptions options, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

        _options = options;
        _resourceGraph = resourceGraph;
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
        ArgumentGuard.NotNull(application, nameof(application));

        foreach (ControllerModel controller in application.Controllers)
        {
            bool isOperationsController = IsOperationsController(controller.ControllerType);

            if (!isOperationsController)
            {
                Type? resourceClrType = ExtractResourceClrTypeFromController(controller.ControllerType);

                if (resourceClrType != null)
                {
                    ResourceType? resourceType = _resourceGraph.FindResourceType(resourceClrType);

                    if (resourceType != null)
                    {
                        if (_controllerPerResourceTypeMap.ContainsKey(resourceType))
                        {
                            throw new InvalidConfigurationException($"Multiple controllers found for resource type '{resourceType}'.");
                        }

                        _resourceTypePerControllerTypeMap.Add(controller.ControllerType, resourceType);
                        _controllerPerResourceTypeMap.Add(resourceType, controller);
                    }
                    else
                    {
                        throw new InvalidConfigurationException($"Controller '{controller.ControllerType}' depends on " +
                            $"resource type '{resourceClrType}', which does not exist in the resource graph.");
                    }
                }
            }

            if (!IsRoutingConventionEnabled(controller))
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

    private bool IsRoutingConventionEnabled(ControllerModel controller)
    {
        return controller.ControllerType.IsSubclassOf(typeof(CoreJsonApiController)) &&
            controller.ControllerType.GetCustomAttribute<DisableRoutingConventionAttribute>(true) == null;
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
        Type baseControllerType = typeof(BaseJsonApiController<,>);
        Type? currentType = controllerType;

        while (!currentType.IsGenericType || currentType.GetGenericTypeDefinition() != baseControllerType)
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
