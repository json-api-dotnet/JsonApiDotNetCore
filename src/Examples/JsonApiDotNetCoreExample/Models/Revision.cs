using System;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Revision : Identifiable
    {
        [Attr]
        public DateTime PublishTime { get; set; }

        [HasOne]
        public Author Author { get; set; }

        [HasOne]
        public Article Article { get; set; }
    }
}
