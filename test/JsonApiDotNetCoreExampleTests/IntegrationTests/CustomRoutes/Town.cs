using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    public sealed class Town : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public double Latitude { get; set; }
        
        [Attr]
        public double Longitude { get; set; }

        [HasMany]
        public ISet<Civilian> Civilians { get; set; }
    }
}
