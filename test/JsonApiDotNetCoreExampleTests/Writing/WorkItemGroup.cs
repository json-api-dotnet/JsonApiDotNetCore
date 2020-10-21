using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.Writing
{
    public sealed class WorkItemGroup : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [HasOne]
        public RgbColor Color { get; set; }

        [Attr]
        public Guid ConcurrencyToken { get; } = Guid.NewGuid();

        [HasMany]
        public IList<WorkItem> Items { get; set; }
    }
}
