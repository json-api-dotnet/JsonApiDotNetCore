using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class Building : Identifiable
    {
        [Attr]
        public string Number { get; set; }

        [Attr]
        public int WindowCount => Windows?.Count ?? 0;

        [Attr]
        public string PrimaryDoorColor
        {
            get
            {
                EnsureHasPrimaryDoor();
                return PrimaryDoor.Color;
            }
            set
            {
                EnsureHasPrimaryDoor();
                PrimaryDoor.Color = value;
            }
        }

        [Attr]
        public string SecondaryDoorColor => SecondaryDoor?.Color;

        [EagerLoad]
        public IList<Window> Windows { get; set; }

        [EagerLoad]
        public Door PrimaryDoor { get; set; }
        
        [EagerLoad]
        public Door SecondaryDoor { get; set; }

        private void EnsureHasPrimaryDoor()
        {
            // Must ensure that an instance exists for this required relationship, so that POST succeeds.
            PrimaryDoor ??= new Door();
        }
    }
}
