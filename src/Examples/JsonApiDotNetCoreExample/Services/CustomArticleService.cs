using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : DefaultResourceService<Article>
    {
        public CustomArticleService(IEnumerable<IQueryParameterService> queryParameters, IJsonApiOptions options,
            ILoggerFactory loggerFactory, IResourceRepository<Article, int> repository,
            IResourceContextProvider provider, IResourceHookExecutor hookExecutor = null)
            : base(queryParameters, options, loggerFactory, repository, provider, hookExecutor)
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
