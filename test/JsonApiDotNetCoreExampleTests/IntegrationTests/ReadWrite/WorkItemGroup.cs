using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemGroup : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsPublic { get; set; }

        [NotMapped]
        [Attr]
        public Guid ConcurrencyToken => Guid.NewGuid();

        [HasOne]
        public RgbColor Color { get; set; }

        [HasMany]
        public IList<WorkItem> Items { get; set; }
    }
}
