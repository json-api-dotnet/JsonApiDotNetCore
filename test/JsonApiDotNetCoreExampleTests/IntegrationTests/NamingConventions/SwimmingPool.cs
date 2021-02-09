using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    public sealed class SwimmingPool : Identifiable
    {
        [Attr]
        public bool IsIndoor { get; set; }

        [HasMany]
        public IList<WaterSlide> WaterSlides { get; set; }

        [HasMany]
        public IList<DivingBoard> DivingBoards { get; set; }
    }
}
