
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Services
{
    public class CustomArticleService : EntityResourceService<Article>
    {
        public CustomArticleService(
            IJsonApiContext jsonApiContext,
            IEntityRepository<Article> repository,
            IJsonApiOptions jsonApiOptions,
            IRequestManager queryManager,
            IPageManager pageManager,
            ILoggerFactory loggerFactory
        ) : base(jsonApiContext, repository, jsonApiOptions, queryManager, pageManager, loggerFactory)
        { }

        public override async Task<Article> GetAsync(int id)
        {
            var newEntity = await base.GetAsync(id);
            newEntity.Name = "None for you Glen Coco";
            return newEntity;
        }
    }

}
