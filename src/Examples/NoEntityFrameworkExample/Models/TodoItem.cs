using System;
using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public sealed class TodoItem : Identifiable
    {
        public TodoItem()
        {
            GuidProperty = Guid.NewGuid();
        }

        public bool IsLocked { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public long Ordinal { get; set; }

        [Attr]
        public Guid GuidProperty { get; set; }
    }
}
