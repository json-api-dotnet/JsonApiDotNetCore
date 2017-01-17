using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<Person> entityRepository,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }
}
