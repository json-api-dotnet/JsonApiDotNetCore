using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing
{
    public sealed class RgbColor : Identifiable<string>
    {
        [Attr]
        public string DisplayName { get; set; }

        // TODO: Change into required relationship and add a test that fails when trying to assign null.
        [HasOne]
        public WorkItemGroup Group { get; set; }
    }
}
