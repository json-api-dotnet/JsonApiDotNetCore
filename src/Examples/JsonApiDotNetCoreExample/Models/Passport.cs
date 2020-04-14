using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        private int? _socialSecurityNumber;

        [Attr]
        public int? SocialSecurityNumber
        {
            get => _socialSecurityNumber;
            set
            {
                if (value != _socialSecurityNumber)
                {
                    LastSocialSecurityNumberChange = DateTime.Now;
                    _socialSecurityNumber = value;
                }
            }
        }

        [Attr]
        public DateTime LastSocialSecurityNumberChange { get; set; }

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
        public string GrantedVisaCountries => GrantedVisas == null || !GrantedVisas.Any()
            ? null
            : string.Join(", ", GrantedVisas.Select(v => v.TargetCountry.Name));

        [EagerLoad]
        public ICollection<Visa> GrantedVisas { get; set; }
    }
}
