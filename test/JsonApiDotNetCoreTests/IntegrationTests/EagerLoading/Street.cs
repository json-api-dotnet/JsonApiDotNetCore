using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.EagerLoading")]
    public sealed class Street : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int BuildingCount => Buildings.Count;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int DoorTotalCount => Buildings.Sum(building => building.SecondaryDoor == null ? 1 : 2);

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public int WindowTotalCount => Buildings.Sum(building => building.WindowCount);

        [EagerLoad]
        public IList<Building> Buildings { get; set; } = new List<Building>();
    }
}
