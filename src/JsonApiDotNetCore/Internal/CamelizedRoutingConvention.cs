// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System;
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

                // derive the targeted resource by reflecting the controllers generic arguments.
                var resourceType = GetResourceTypeFromController(controller.ControllerType);
                string endpoint;
                if (resourceType != null)
                    endpoint = _formatter.FormatResourceName(resourceType);
                else
                    endpoint = _formatter.ApplyCasingConvention(controller.ControllerName);

                // apply the registered resource name formatter to the discovered resource type.
                var template = $"{_namespace}/{endpoint}";
                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel
                {
                    Template = template
                };
            }
        }

        private bool RoutingConventionDisabled(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var notDisabled = type.GetCustomAttribute<DisableRoutingConventionAttribute>() == null;
            return notDisabled && type.IsSubclassOf(typeof(JsonApiControllerMixin));
        }


        public Type GetResourceTypeFromController(Type type)
        {
            var target = typeof(BaseJsonApiController<,>);
            // return all inherited types
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
