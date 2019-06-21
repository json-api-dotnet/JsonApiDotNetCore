using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TodoResource : LockableResourceBase<TodoItem>
    {
        public TodoResource(IResourceGraph graph) : base(graph) { }

        public override void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
        {
            if (stringId == "1337")
            {
                throw new JsonApiException(403, "Not allowed to update author of any TodoItem", new UnauthorizedAccessException());
            }
        }

        public override void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TodoItem> resourcesByRelationship, ResourcePipeline pipeline)
        {
            List<TodoItem> todos = resourcesByRelationship.GetByRelationship<Person>().SelectMany(kvp => kvp.Value).ToList();
            DisallowLocked(todos);
        }
    }
}
