using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public override IEnumerable<Passport> BeforeUpdate(EntityDiff<Passport> entityDiff, HookExecutionContext<Passport> context)
        {
            return base.BeforeUpdate(entityDiff, context);
        }

        public override IEnumerable<Passport> BeforeDelete(IEnumerable<Passport> entities, HookExecutionContext<Passport> context)
        {
            return base.BeforeDelete(entities, context);
        }
    }
}
