using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiQueryController<TResource> : JsonApiQueryController<TResource, int> where TResource : class, IIdentifiable<int>
    {
        protected JsonApiQueryController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, int> queryService)
            : base(options, loggerFactory, queryService)
        { }
    }

    public abstract class JsonApiQueryController<TResource, TId> : BaseJsonApiController<TResource, TId> where TResource : class, IIdentifiable<TId>
    {
        protected JsonApiQueryController(
            IJsonApiOptions context,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, TId> queryService)
            : base(context, loggerFactory, queryService)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);

        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName)
            => await base.GetSecondaryAsync(id, relationshipName);
    }
}
