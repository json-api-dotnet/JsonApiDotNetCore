using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TagHooksDefinition : ResourceHooksDefinition<Tag>
    {
        public TagHooksDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        public override IEnumerable<Tag> OnReturn(HashSet<Tag> resources, ResourcePipeline pipeline)
        {
            return resources.Where(tag => tag.Name != "This should not be included").ToArray();
        }
    }
}
