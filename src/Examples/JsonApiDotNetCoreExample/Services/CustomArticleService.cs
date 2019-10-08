using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : EntityResourceService<Article>
    {
        public CustomArticleService(IEntityRepository<Article, int> repository, IJsonApiOptions options, ITargetedFields updatedFields, ICurrentRequest currentRequest, IIncludeService includeService, ISparseFieldsService sparseFieldsService, IPageQueryService pageManager, IResourceGraph resourceGraph, IResourceHookExecutor hookExecutor = null, IResourceMapper mapper = null, ILoggerFactory loggerFactory = null) : base(repository, options, updatedFields, currentRequest, includeService, sparseFieldsService, pageManager, resourceGraph, hookExecutor, mapper, loggerFactory)
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
