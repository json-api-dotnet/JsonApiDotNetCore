#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemGroup : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsPublic { get; set; }

        [NotMapped]
        [Attr]
        public bool IsDeprecated => Name != null && Name.StartsWith("DEPRECATED:", StringComparison.OrdinalIgnoreCase);

        [HasOne]
        public RgbColor Color { get; set; }

        [HasMany]
        public IList<WorkItem> Items { get; set; }
    }
}
