using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TodoItemHooksDefinition : LockableHooksDefinition<TodoItem>
    {
        public TodoItemHooksDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

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
            List<TodoItem> todoItems = resourcesByRelationship.GetByRelationship<Person>().SelectMany(pair => pair.Value).ToList();
            DisallowLocked(todoItems);
        }

        public override IEnumerable<TodoItem> OnReturn(HashSet<TodoItem> resources, ResourcePipeline pipeline)
        {
            return resources.Where(todoItem => todoItem.Description != "This should not be included").ToArray();
        }
    }
}
