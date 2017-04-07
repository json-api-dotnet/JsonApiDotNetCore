// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
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
                if (IsJsonApiController(controller))
                {
                    var template = $"{_namespace}/{controller.ControllerName.Dasherize()}";
                    controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel()
                    {
                        Template = template
                    };
                }
            }
        }

        private bool IsJsonApiController(ControllerModel controller) 
        {
            var controllerBaseType = controller.ControllerType.BaseType;
            if(!controllerBaseType.IsConstructedGenericType) return false;
            var genericTypeDefinition = controllerBaseType.GetGenericTypeDefinition();
            return (genericTypeDefinition == typeof(JsonApiController<,>) || genericTypeDefinition == typeof(JsonApiController<>));
        }
    }
}
