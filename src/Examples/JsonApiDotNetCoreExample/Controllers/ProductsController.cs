using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class ProductsController: JsonApiController<Product>
    {
        public ProductsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IDbContextResolver contextResolver,
            IResourceService<Product> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { 
        }        
    }
}
