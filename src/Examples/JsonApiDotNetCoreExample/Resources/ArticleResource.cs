using System.Collections.Generic;
using System.Linq;
using System;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;
using System.Security.Principal;

//namespace JsonApiDotNetCoreExample.Resources
//{
//    public class ArticleResource : ResourceDefinition<Article>
//    {
//        public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
//        {
//            if (pipeline == ResourcePipeline.ReadSingle && entities.Single().Name == "Classified")
//            {
//                throw new JsonApiException(403, "You are not allowed to see this article!", new UnauthorizedAccessException());
//            }
//            return entities.Where(t => t.Name != "This should be not be included");
//        }
//    }
//}



//namespace JsonApiDotNetCoreExample.Resources
//{

//    /// This is a scoped service, which means wil log will have a request-based
//    /// unique id associated to it.
//    public class UserActionsLogger : IUserActionsLogger
//    {
//        public ILogger GetLogger { get; private set; }
//        public UserActionsLogger(ILoggerFactory loggerFactory,
//                                 IUserService userService)
//        {
//            var userId = userService.GetUser().Id;
//            GetLogger = loggerFactory.CreateLogger($"[request: {Guid.NewGuid()}, user: {userId}] User Actions Log:");
//        }
//    }

//    /// Resource definitions are also injected as scoped services.
//    public class ArticleResource : ResourceDefinition<Article>
//    {

//        public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
//        {
//            return entities.Where(a => a.SoftDeleted == false);
//        }

//    }

//    public class PersonResource : ResourceDefinition<Person>
//    {
//        public override IEnumerable<Person> OnReturn(HashSet<Person> entities, ResourcePipeline pipeline)
//        {
//            if (pipeline == ResourcePipeline.Get || pipeline == ResourcePipeline.GetSingle)
//            {
//                return entities.Where(p => !p.WantsPrivacy);
//            }
//            return entities;
//        }
//    }
//}
