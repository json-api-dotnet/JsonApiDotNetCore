using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        private readonly ISystemClock _systemClock;
        private int? _socialSecurityNumber;

        protected override string GetStringId(object value)
        {
            return HexadecimalObfuscationCodec.Encode(value);
        }

        protected override int GetTypedId(string value)
        {
            return HexadecimalObfuscationCodec.Decode(value);
        }

        [Attr]
        public int? SocialSecurityNumber
        {
            get => _socialSecurityNumber;
            set
            {
                if (value != _socialSecurityNumber)
                {
                    LastSocialSecurityNumberChange = _systemClock.UtcNow.LocalDateTime;
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

        public Passport(AppDbContext appDbContext)
        {
            _systemClock = appDbContext.SystemClock;
        }
    }
}
