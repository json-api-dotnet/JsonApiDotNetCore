using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

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
