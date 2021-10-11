#nullable disable

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
        [Required]
        [MinLength(1)]
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ISet<TodoItem> TodoItems { get; set; }
    }
}
