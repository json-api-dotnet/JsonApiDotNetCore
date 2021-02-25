using System;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Car : Identifiable<string>
    {
        [NotMapped]
        public override string Id
        {
            get => $"{RegionId}:{LicensePlate}";
            set
            {
                string[] elements = value.Split(':');

                if (elements.Length == 2)
                {
                    if (int.TryParse(elements[0], out int regionId))
                    {
                        RegionId = regionId;
                        LicensePlate = elements[1];
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Failed to convert ID '{value}'.");
                }
            }
        }

        [Attr]
        public string LicensePlate { get; set; }

        [Attr]
        public long RegionId { get; set; }

        [HasOne]
        public Engine Engine { get; set; }

        [HasOne]
        public Dealership Dealership { get; set; }
    }
}
