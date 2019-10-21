// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// The default routing convention registers the name of the resource as the route
    /// using the <see cref="IResourceNameFormatter"/> that is registered. The default for this is
    /// a kebab-case formatter. If the controller directly inherits from JsonApiMixin and there is no
    /// resource directly associated, it used the name of the controller instead of the name of the type.
    /// </summary>
    /// <example>
    /// public class SomeResourceController: JsonApiController{SomeResource} { }
    /// // => /some-resources/relationship/related-resource
    ///
    /// public class RandomNameController{SomeResource} : JsonApiController{SomeResource} { }
    /// // => /some-resources/relationship/related-resource
    ///
    /// // when using the camelCase formatter:
    /// public class SomeResourceController{SomeResource} : JsonApiController{SomeResource} { }
    /// // => /someResources/relationship/relatedResource
    ///
    /// // when inheriting from JsonApiMixin formatter:
    /// public class SomeVeryCustomController{SomeResource} : JsonApiMixin { }
    /// // => /some-very-customs/relationship/related-resource
    /// </example>
    public class DefaultRoutingConvention : IJsonApiRoutingConvention
    {
        private readonly string _namespace;
        private readonly IResourceNameFormatter _formatter;
        private readonly HashSet<string> _registeredTemplates = new HashSet<string>();
        private readonly Dictionary<string, Type> _registeredResources = new Dictionary<string, Type>();
        public DefaultRoutingConvention(IJsonApiOptions options, IResourceNameFormatter formatter)
        {
            _namespace = options.Namespace;
            _formatter = formatter;
        }

        /// <inheritdoc/>
        public Type GetAssociatedResource(string controllerName)
        {
            _registeredResources.TryGetValue(controllerName, out Type type);
            return type;
        }

        /// <inheritdoc/>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var resourceType = GetResourceTypeFromController(controller.ControllerType);
                if (resourceType != null)
                    _registeredResources.Add(controller.ControllerName, resourceType);

                if (RoutingConventionDisabled(controller) == false)
                    continue;

                var template = TemplateFromResource(controller) ?? TemplateFromController(controller);
                if (template == null)
                    throw new JsonApiSetupException($"Controllers with overlapping route templates detected: {controller.ControllerType.FullName}");

                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel { Template = template };
            }
        }

        /// <summary>
        /// verifies if routing convention should be enabled for this controller
        /// </summary>
        private bool RoutingConventionDisabled(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var notDisabled = type.GetCustomAttribute<DisableRoutingConventionAttribute>() == null;
            return notDisabled && type.IsSubclassOf(typeof(JsonApiControllerMixin));
        }

        /// <summary>
        /// Derives a template from the resource type, and checks if this template was already registered.
        /// </summary>
        private string TemplateFromResource(ControllerModel model)
        {
            if (_registeredResources.TryGetValue(model.ControllerName, out Type resourceType))
            {
                var template = $"{_namespace}/{_formatter.FormatResourceName(resourceType)}";
                if (_registeredTemplates.Add(template))                
                    return template;

            }
            return null;
        }

        /// <summary>
        /// Derives a template from the controller name, and checks if this template was already registered.
        /// </summary>
        private string TemplateFromController(ControllerModel model)
        {
            var template = $"{_namespace}/{_formatter.ApplyCasingConvention(model.ControllerName)}";
            if (_registeredTemplates.Add(template))
                return template;
            return null;
        }

        /// <summary>
        /// Determines the resource associated to a controller by inspecting generic arguments.
        /// </summary>
        private Type GetResourceTypeFromController(Type type)
        {
            var controllerBase = typeof(ControllerBase);
            var jsonApiMixin = typeof(JsonApiControllerMixin);
            var target = typeof(BaseJsonApiController<,>);
            var identifiable = typeof(IIdentifiable);
            var currentBaseType = type;
            if (type.Name.Contains("TodoItemsCustom"))
            {
                var x = 123;
            }
                    
            while (!currentBaseType.IsGenericType || currentBaseType.GetGenericTypeDefinition() != target)
            {
                var nextBaseType = currentBaseType.BaseType;

                if ( (nextBaseType == controllerBase || nextBaseType == jsonApiMixin) && currentBaseType.IsGenericType)
                {
                    var potentialResource = currentBaseType.GetGenericArguments().FirstOrDefault(t => t.Inherits(identifiable));
                    if (potentialResource != null)
                        return potentialResource;
                }

                currentBaseType = nextBaseType;
                if (nextBaseType == null)
                    break;
            }
            return currentBaseType?.GetGenericArguments().First();
        }
    }
}
