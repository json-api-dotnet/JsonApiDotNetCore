using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiQueryController<T> : JsonApiQueryController<T, int> where T : class, IIdentifiable<int>
    {
        protected JsonApiQueryController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceQueryService<T, int> queryService)
            : base(jsonApiOptions, loggerFactory, queryService)
        { }
    }

    public abstract class JsonApiQueryController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        protected JsonApiQueryController(
            IJsonApiOptions jsonApiContext,
            ILoggerFactory loggerFactory,
            IResourceQueryService<T, TId> queryService)
            : base(jsonApiContext, loggerFactory, queryService)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
            => await base.GetRelationshipsAsync(id, relationshipName);

        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);
    }
}
