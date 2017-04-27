// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    public class DasherizedRoutingConvention : IApplicationModelConvention
    {
        private string _namespace;
        public DasherizedRoutingConvention(string nspace)
        {
            _namespace = nspace;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var template = string.Empty;
                
                if (IsDasherizedJsonApiController(controller))
                    template = $"{_namespace}/{controller.ControllerName.Dasherize()}";
                else 
                    template = GetTemplate(controller);

                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel()
                {
                    Template = template
                };
            }
        }

        private bool IsDasherizedJsonApiController(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var notDisabled = type.GetCustomAttribute<DisableRoutingConventionAttribute>() == null;
            return notDisabled && type.IsSubclassOf(typeof(JsonApiControllerMixin));
        }

        private string GetTemplate(ControllerModel controller)
        {
            var type = controller.ControllerType;
            var routeAttr = type.GetCustomAttribute<RouteAttribute>();
            if(routeAttr != null)
                return ((RouteAttribute)routeAttr).Template;

            return controller.ControllerName;
        }
    }
}
