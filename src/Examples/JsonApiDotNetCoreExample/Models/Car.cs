using System;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Car : Identifiable<string>
    {
        [NotMapped]
        public override string Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        protected override string GetStringId(string value)
        {
            return $"{RegionId}:{LicensePlate}";
        }

        protected override string GetTypedId(string value)
        {
            var elements = value.Split(':');
            if (elements.Length == 2)
            {
                if (int.TryParse(elements[0], out int regionId))
                {
                    RegionId = regionId;
                    LicensePlate = elements[1];
                    return value;
                }
            }

            throw new InvalidOperationException($"Failed to convert ID '{value}'.");
        }

        [Attr]
        public string LicensePlate { get; set; }

        [Attr]
        public long RegionId { get; set; }

        [HasOne]
        public Engine Engine { get; set; }
    }
}
