using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : EntityResourceService<Article>
    {
        public CustomArticleService(
            IEntityRepository<Article> repository,
            IJsonApiOptions jsonApiOptions,
            IRequestManager queryManager,
            IPageManager pageManager,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory = null
        ) : base(repository: repository, jsonApiOptions, queryManager, pageManager, resourceGraph:resourceGraph, loggerFactory)
        { }

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
