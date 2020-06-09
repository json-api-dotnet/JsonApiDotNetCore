using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using JsonApiDotNetCore.Models.CustomValidators;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Author : Identifiable
    {
        [Attr]
        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; }

        [HasMany]
        public IList<Article> Articles { get; set; }
    }
}

