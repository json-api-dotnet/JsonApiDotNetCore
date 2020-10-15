using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Car : IIdentifiable<string>
    {
        public string Id
        {
            get => $"{RegionId}:{LicensePlate}";
            set
            {
                var elements = value.Split(':');
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

        public string StringId
        {
            get => Id;
            set => Id = value;
        }

        [Attr] public string LicensePlate { get; set; }

        [Attr] public long? RegionId { get; set; }

        [HasOne] public Engine Engine { get; set; }
    }
}
