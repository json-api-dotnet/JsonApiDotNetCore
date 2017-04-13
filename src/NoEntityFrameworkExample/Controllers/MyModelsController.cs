using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Controllers
{
    public class MyModelsController : JsonApiController<MyModel>
    {
        public MyModelsController(
            IJsonApiContext jsonApiContext, 
            IResourceService<MyModel> resourceService, 
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
