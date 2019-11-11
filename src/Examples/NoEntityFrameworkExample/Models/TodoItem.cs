using System;
using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class TodoItem : Identifiable
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

        [Attr]
        public DateTime CreatedDate { get; set; }

        [Attr(isFilterable: false, isSortable: false)]
        public DateTime? AchievedDate { get; set; }

        [Attr]
        public DateTime? UpdatedDate { get; set; }

        [Attr]
        public DateTimeOffset? OffsetDate { get; set; }
    }
}
