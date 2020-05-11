using System;
using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public sealed class TodoItem : Identifiable
    {
        [Attr]
        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public long Ordinal { get; set; }

        [Attr]
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }
}
