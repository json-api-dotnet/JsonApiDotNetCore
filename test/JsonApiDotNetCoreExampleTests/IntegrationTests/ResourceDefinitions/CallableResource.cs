using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions
{
    public sealed class CallableResource : Identifiable
    {
        [Attr]
        public string Label { get; set; }

        [Attr]
        public int PercentageComplete { get; set; }

        [Attr]
        public string Status => $"{PercentageComplete}% completed.";

        [Attr]
        public int RiskLevel { get; set; }

        [Attr(AttrCapabilities.AllowView | AttrCapabilities.AllowSort)]
        public DateTime CreatedAt { get; set; }

        [Attr(AttrCapabilities.AllowView | AttrCapabilities.AllowSort)]
        public DateTime ModifiedAt { get; set; }

        [Attr(AttrCapabilities.None)]
        public bool IsDeleted { get; set; }

        [HasMany]
        public ICollection<CallableResource> Children { get; set; }

        [HasOne]
        public CallableResource Owner { get; set; }
    }
}
