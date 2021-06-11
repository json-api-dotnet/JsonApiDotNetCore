using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TodoItem : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [HasMany]
        public ISet<Person> Stakeholders { get; set; }
    }
}
