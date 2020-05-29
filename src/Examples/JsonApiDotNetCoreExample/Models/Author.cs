using System;
using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Author : Identifiable
    {
        [Attr]
        public string FirstName { get; set; }

        [Attr]
        [IsRequired(AllowEmptyStrings = true)]
        public string LastName { get; set; }

        [Attr]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string BusinessEmail { get; set; }

        [HasOne]
        public Address LivingAddress { get; set; }

        [HasMany]
        public IList<Article> Articles { get; set; }
    }
}
