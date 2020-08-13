using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : JsonApiResourceService<Article>
    {
        public CustomArticleService(
            IResourceRepository<Article> repository,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            ICurrentRequest currentRequest,
            IResourceChangeTracker<Article> resourceChangeTracker,
            IResourceFactory resourceFactory,
            IResourceHookExecutor hookExecutor = null)
            : base(repository, queryLayerComposer, paginationContext, options, loggerFactory, currentRequest,
                resourceChangeTracker, resourceFactory, hookExecutor)
        { }

        public override async Task<Article> GetAsync(int id)
        {
            var resource = await base.GetAsync(id);
            resource.Caption = "None for you Glen Coco";
            return resource;
        }
    }
}
