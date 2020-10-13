using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Category : Identifiable<string>
    {
        public override string Id
        {
            get => $"{CountryId}-{ShopId}";
            set
            {
                var elements = value.Split('-');
                if (elements.Length == 2)
                {
                    if (int.TryParse(elements[0], out var countryId))
                    {
                        CountryId = countryId;
                        ShopId = elements[1];
                        return;
                    }
                }

                throw new InvalidOperationException($"Failed to convert ID '{value}'.");
            }
        }

        [Attr]
        public string Name { get; set; }

        [HasMany]
        public ICollection<Product> Products { get; set; }
       
        public int? CountryId { get; set; }

        public string ShopId { get; set; }
        
    }
}
