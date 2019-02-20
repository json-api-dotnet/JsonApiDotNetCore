using JsonApiDotNetCore.Logic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExample.Logic
{
    public class LogicCache : BaseLogicCache
    {

        /// <summary>
        /// Hacky, but fast to show how it could work
        /// </summary>
        /// <param name="tagDefinition"></param>
        public LogicCache(ResourceDefinition<Tag> tagDefinition)
        {
            _cache.Add(typeof(Tag), tagDefinition);
        }

    }
}
