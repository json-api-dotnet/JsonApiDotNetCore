using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : DefaultResourceService<Article>
    {
        public CustomArticleService(ISortService sortService,
                                    IFilterService filterService,
                                    IResourceRepository<Article, int> repository,
                                    IJsonApiOptions options,
                                    IIncludeService includeService,
                                    ISparseFieldsService sparseFieldsService,
                                    IPageService pageService,
                                    IContextEntityProvider provider,
                                    IResourceHookExecutor hookExecutor = null,
                                    ILoggerFactory loggerFactory = null)
            : base(sortService, filterService, repository, options, includeService, sparseFieldsService,
                   pageService, provider, hookExecutor, loggerFactory)
        {
        }

        public override async Task<Article> GetAsync(int id)
        {
            var newEntity = await base.GetAsync(id);
            if(newEntity == null)
            {
                throw new JsonApiException(404, "The entity could not be found");
            }
            newEntity.Name = "None for you Glen Coco";
            return newEntity;
        }
    }

}
