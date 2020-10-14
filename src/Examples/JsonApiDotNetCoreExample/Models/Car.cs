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
            return $"{RegionCode}:{LicensePlate}";
        }

        protected override string GetTypedId(string value)
        {
            var elements = value.Split(':');
            if (elements.Length == 2)
            {
                RegionCode = elements[0];
                LicensePlate = elements[1];
                return value;
            }

            throw new InvalidOperationException($"Failed to convert ID '{value}'.");
        }

        [Attr]
        public string LicensePlate { get; set; }

        [Attr]
        public string RegionCode { get; set; }

        [HasOne]
        public Engine Engine { get; set; }
    }
}
