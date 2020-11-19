using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        private readonly ISystemClock _systemClock;
        private int? _socialSecurityNumber;

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
            get => BirthCountry?.Name;
            set
            {
                BirthCountry ??= new Country();
                BirthCountry.Name = value;
            }
        }

        [EagerLoad]
        public Country BirthCountry { get; set; }

        public Passport(AppDbContext appDbContext)
        {
            _systemClock = appDbContext.SystemClock;
        }
    }
}
