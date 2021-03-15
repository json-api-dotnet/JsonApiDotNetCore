using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    public sealed class StoresController : JsonApiController<Store>
    {
        public StoresController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Store> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [HttpGet]
        [Produces("application/vnd.api+json")]
        [ProducesResponseType(typeof(IEnumerable<Store>), 200)]
        public override Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            return base.GetAsync(cancellationToken);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Store), 200)]
        public override Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
        {
            return base.GetAsync(id, cancellationToken);
        }

        [HttpPost]
        [Consumes("application/vnd.api+json")]
        public override async Task<IActionResult> PostAsync([FromBody] Store resource, CancellationToken cancellationToken)
        {
            return await base.PostAsync(resource, cancellationToken);
        }

        [HttpPost]
        [Consumes("application/vnd.api+json; ext=\"https://jsonapi.org/ext/atomic\"")]
        public override Task<IActionResult> PostRelationshipAsync(int id, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            return base.PostRelationshipAsync(id, relationshipName, secondaryResourceIds, cancellationToken);
        }
    }
}
