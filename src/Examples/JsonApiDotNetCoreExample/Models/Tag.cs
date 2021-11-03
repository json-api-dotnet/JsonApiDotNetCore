using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Tag : Identifiable<int>
    {
        [Attr]
        [MinLength(1)]
        public string Name { get; set; } = null!;

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; } = new HashSet<TodoItem>();
    }
}
