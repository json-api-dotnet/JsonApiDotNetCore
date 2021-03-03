using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Building : Identifiable
    {
        private string _tempPrimaryDoorColor;

        [Attr]
        public string Number { get; set; }

        [NotMapped]
        [Attr]
        public int WindowCount => Windows?.Count ?? 0;

        [NotMapped]
        [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowChange)]
        public string PrimaryDoorColor
        {
            get => _tempPrimaryDoorColor ?? PrimaryDoor.Color;
            set
            {
                if (PrimaryDoor == null)
                {
                    // A request body is being deserialized. At this time, related entities have not been loaded.
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
        public string SecondaryDoorColor => SecondaryDoor?.Color;

        [EagerLoad]
        public IList<Window> Windows { get; set; }

        [EagerLoad]
        public Door PrimaryDoor { get; set; }

        [EagerLoad]
        public Door SecondaryDoor { get; set; }
    }
}
