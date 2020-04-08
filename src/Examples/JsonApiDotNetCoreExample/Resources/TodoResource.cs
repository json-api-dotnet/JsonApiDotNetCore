using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TodoResource : LockableResource<TodoItem>
    {
        public TodoResource(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
        {
            if (stringId == "1337")
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "You are not allowed to update the author of todo items."
                });
            }
        }

        public override void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TodoItem> resourcesByRelationship, ResourcePipeline pipeline)
        {
            List<TodoItem> todos = resourcesByRelationship.GetByRelationship<Person>().SelectMany(kvp => kvp.Value).ToList();
            DisallowLocked(todos);
        }
    }
}
