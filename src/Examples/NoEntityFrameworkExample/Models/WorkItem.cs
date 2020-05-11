using System;
using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public sealed class WorkItem : Identifiable
    {
        [Attr]
        public bool IsBlocked { get; set; }

        [Attr]
        public string Title { get; set; }

        [Attr]
        public long DurationInHours { get; set; }

        [Attr]
        public Guid ProjectId { get; set; } = Guid.NewGuid();
    }
}
