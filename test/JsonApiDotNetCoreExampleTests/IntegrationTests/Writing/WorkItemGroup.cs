using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing
{
    // TODO: What does a WorkItemGroup represent? I'm not so sure about this being an intuitive model like Article with Authors etc. 
    public sealed class WorkItemGroup : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsPublic { get; set; }

        [NotMapped]
        [Attr]
        public Guid ConcurrencyToken { get; } = Guid.NewGuid();

        [HasOne]
        public RgbColor Color { get; set; }

        [HasMany]
        public IList<WorkItem> Items { get; set; }
    }
}
