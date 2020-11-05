using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : JsonApiResourceService<Article>
    {
        public CustomArticleService(
            IResourceRepository<Article> repository,
            IGetResourcesByIds getResourcesById,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<Article> resourceChangeTracker,
            IResourceFactory resourceFactory,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceHookExecutorFacade hookExecutor)
            : base(repository, getResourcesById, queryLayerComposer, paginationContext, options, loggerFactory,
                request, resourceChangeTracker, resourceFactory, targetedFields, resourceContextProvider, hookExecutor)
        { }

        public override async Task<Article> GetAsync(int id)
        {
            var resource = await base.GetAsync(id);
            resource.Caption = "None for you Glen Coco";
            return resource;
        }
    }
}
