using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class Map : Identifiable<Guid?>
    {
        [Attr]
        public string Name { get; set; }

        [HasOne]
        public Game Game { get; set; }
    }
}
