// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    public class CamelizedRoutingConvention : IApplicationModelConvention
    {
        private readonly string _namespace;
        public CamelizedRoutingConvention(string nspace)
        {
            _namespace = nspace;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (IsCamelizedJsonApiController(controller) == false)
                    continue;

                var template = $"{_namespace}/{controller.ControllerName.Camelize()}";
                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel
                {
                    Template = template
                };
            }
        }

        private bool IsCamelizedJsonApiController(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var notDisabled = type.GetCustomAttribute<DisableRoutingConventionAttribute>() == null;
            return notDisabled && type.IsSubclassOf(typeof(JsonApiControllerMixin));
        }
    }
}
