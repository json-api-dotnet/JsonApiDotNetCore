// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Graph;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// The default routing convention registers the name of the resource as the route
    /// using the <see cref="IResourceNameFormatter"/> that is registered. The default for this is
    /// a kebab-case formatter.
    /// </summary>
    /// <example>
    /// public class SomeResourceController{SomeResource} : JsonApiController{SomeResource} { }
    /// // => /some-resources/relationship/related-resource
    ///
    /// public class RandomNameController{SomeResource} : JsonApiController{SomeResource} { }
    /// // => /some-resources/relationship/related-resource
    ///
    /// // when using the camelCase formatter:
    /// public class SomeResourceController{SomeResource} : JsonApiController{SomeResource} { }
    /// // => /someResources/relationship/relatedResource
    /// </example>
    public class DefaultRoutingConvention : IJsonApiRoutingConvention
    {
        private readonly string _namespace;
        private readonly IResourceNameFormatter _formatter;
        private readonly HashSet<string> _registeredTemplates = new HashSet<string>();
        public DefaultRoutingConvention(IJsonApiOptions options, IResourceNameFormatter formatter)
        {
            _namespace = options.Namespace;
            _formatter = formatter;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (RoutingConventionDisabled(controller) == false)
                    continue;

                var template = TemplateFromResource(controller) ?? TemplateFromController(controller);
                if (template == null)
                    throw new JsonApiSetupException($"Controllers with overlapping route templates detected: {controller.ControllerType.FullName}");

                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel { Template = template };
            }
        }

        private bool RoutingConventionDisabled(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var notDisabled = type.GetCustomAttribute<DisableRoutingConventionAttribute>() == null;
            return notDisabled && type.IsSubclassOf(typeof(JsonApiControllerMixin));
        }

        private string TemplateFromResource(ControllerModel model)
        {
            var resourceType = GetResourceTypeFromController(model.ControllerType);
            if (resourceType != null)
            {
                var template = $"{_namespace}/{_formatter.FormatResourceName(resourceType)}";
                if (_registeredTemplates.Add(template))
                    return template;
            }
            return null;
        }

        private string TemplateFromController(ControllerModel model)
        {
            var template = $"{_namespace}/{_formatter.ApplyCasingConvention(model.ControllerName)}";
            if (_registeredTemplates.Add(template))
                return template;
            return null;
        }

        private Type GetResourceTypeFromController(Type type)
        {
            var target = typeof(BaseJsonApiController<,>);
            var currentBaseType = type.BaseType;
            while (!currentBaseType.IsGenericType || currentBaseType.GetGenericTypeDefinition() != target)
            {
                currentBaseType = currentBaseType.BaseType;
                if (currentBaseType == null) break;
            }
            return currentBaseType?.GetGenericArguments().First();
        }
    }
}
