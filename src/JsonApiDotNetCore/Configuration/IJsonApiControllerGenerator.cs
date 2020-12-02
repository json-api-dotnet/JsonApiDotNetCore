using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JsonApiDotNetCore.Configuration
{
    // TODO add comments
    public interface IJsonApiControllerGenerator : IApplicationFeatureProvider<ControllerFeature> { }
}
