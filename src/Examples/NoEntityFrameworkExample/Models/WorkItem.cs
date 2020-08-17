using System;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

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
