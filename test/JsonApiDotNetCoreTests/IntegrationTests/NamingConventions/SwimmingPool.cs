using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
