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

        [Attr("description")]
        public string Description { get; set; }

        [Attr("ordinal")]
        public long Ordinal { get; set; }

        [Attr("guid-property")]
        public Guid GuidProperty { get; set; }

        [Attr("created-date")]
        public DateTime CreatedDate { get; set; }

        [Attr("achieved-date", isFilterable: false, isSortable: false)]
        public DateTime? AchievedDate { get; set; }

        [Attr("updated-date")]
        public DateTime? UpdatedDate { get; set; }

        [Attr("offset-date")]
        public DateTimeOffset? OffsetDate { get; set; }
    }
}
