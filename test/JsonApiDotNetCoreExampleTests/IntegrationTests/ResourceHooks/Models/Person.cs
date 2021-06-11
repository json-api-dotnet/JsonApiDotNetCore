using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable, IIsLockable
    {
        public bool IsLocked { get; set; }

        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }

        [HasOne]
        public TodoItem StakeholderTodoItem { get; set; }

        [HasOne]
        public Passport Passport { get; set; }
    }
}
