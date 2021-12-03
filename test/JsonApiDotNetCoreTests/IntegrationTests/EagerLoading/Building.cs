using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.EagerLoading")]
    public sealed class Building : Identifiable<int>
    {
        private string? _tempPrimaryDoorColor;

        [Attr]
        public string Number { get; set; } = null!;

        [NotMapped]
        [Attr]
        public int WindowCount => Windows.Count;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public string PrimaryDoorColor
        {
            get
            {
                if (_tempPrimaryDoorColor == null && PrimaryDoor == null)
                {
                    // The ASP.NET model validator reads the value of this required property, to ensure it is not null.
                    // When creating a resource, BuildingDefinition ensures a value is assigned. But when updating a resource
                    // and PrimaryDoorColor is explicitly set to null in the request body and ModelState validation is enabled,
                    // we want it to produce a validation error, so return null here.
                    return null!;
                }

                return _tempPrimaryDoorColor ?? PrimaryDoor!.Color;
            }
            set
            {
                if (PrimaryDoor == null)
                {
                    // A request body is being deserialized. At this time, related entities have not been loaded yet.
                    // We cache the assigned value in a private field, so it can be used later.
                    _tempPrimaryDoorColor = value;
                }
                else
                {
                    PrimaryDoor.Color = value;
                }
            }
        }

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView)]
        public string? SecondaryDoorColor => SecondaryDoor?.Color;

        [EagerLoad]
        public IList<Window> Windows { get; set; } = new List<Window>();

        [EagerLoad]
        public Door? PrimaryDoor { get; set; }

        [EagerLoad]
        public Door? SecondaryDoor { get; set; }
    }
}
