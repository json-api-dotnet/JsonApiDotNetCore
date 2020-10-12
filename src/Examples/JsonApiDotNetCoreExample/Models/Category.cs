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
                var split = value.Split('-');
                CountryId = int.Parse(split[0]);
                ShopId =  split[1];
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
