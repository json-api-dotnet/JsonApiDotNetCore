using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class Street : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public int BuildingCount => Buildings?.Count ?? 0;

        [Attr]
        public int DoorTotalCount => Buildings?.Sum(building => building.SecondaryDoor == null ? 1 : 2) ?? 0;

        [Attr]
        public int WindowTotalCount => Buildings?.Sum(building => building.WindowCount) ?? 0;

        [EagerLoad]
        public IList<Building> Buildings { get; set; }
    }
}
