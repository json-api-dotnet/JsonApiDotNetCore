using System;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Data;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        [RegularExpression(@"^\W$")]
        public string Name { get; set; }

        public Tag(AppDbContext appDbContext)
        {
            if (appDbContext == null) throw new ArgumentNullException(nameof(appDbContext));
        }
    }
}
