using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Passport : Identifiable
    {
        [Attr]
        public int? SocialSecurityNumber { get; set; }

        [Attr]
        public bool IsLocked { get; set; }

        [HasOne]
        public Person Person { get; set; }

        [Attr]
        [NotMapped]
        public string BirthCountryName
        {
            get => BirthCountry.Name;
            set
            {
                if (BirthCountry == null)
                {
                    BirthCountry = new Country();
                }
                
                BirthCountry.Name = value;
            }
        }

        [EagerLoad]
        public Country BirthCountry { get; set; }

        [Attr(isImmutable: true)]
        [NotMapped]
        public string GrantedVisaCountries
        {
            get => GrantedVisas == null ? null : string.Join(", ", GrantedVisas.Select(v => v.TargetCountry.Name));
            // The setter is required only for deserialization in unit tests.
            set { }
        }

        [EagerLoad]
        public ICollection<Visa> GrantedVisas { get; set; }
    }
}