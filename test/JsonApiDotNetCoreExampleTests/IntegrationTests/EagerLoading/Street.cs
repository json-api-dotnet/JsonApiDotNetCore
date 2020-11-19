using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class Street : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int BuildingCount => Buildings?.Count ?? 0;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int DoorTotalCount => Buildings?.Sum(building => building.SecondaryDoor == null ? 1 : 2) ?? 0;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int WindowTotalCount => Buildings?.Sum(building => building.WindowCount) ?? 0;

        [EagerLoad]
        public IList<Building> Buildings { get; set; }
    }
}
