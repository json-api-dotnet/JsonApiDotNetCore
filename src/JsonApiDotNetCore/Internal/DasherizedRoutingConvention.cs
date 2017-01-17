// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
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
                var template = $"{_namespace}/{controller.ControllerName.Dasherize()}";
                controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel()
                {
                    Template = template
                };
            }
        }
    }
}
