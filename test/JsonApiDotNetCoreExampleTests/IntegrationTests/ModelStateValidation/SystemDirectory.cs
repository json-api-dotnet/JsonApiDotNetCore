using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class SystemDirectory : Identifiable
    {
        [Attr]
        [IsRequired]
        [RegularExpression(@"^[\w\s]+$")]
        public string Name { get; set; }

        [Attr]
        [IsRequired]
        public bool? IsCaseSensitive { get; set; }

        [Attr]
        [Range(typeof(long), "0", "9223372036854775807")]
        public long SizeInBytes { get; set; }

        [HasMany] 
        public ICollection<SystemDirectory> Subdirectories { get; set; }

        [HasMany] 
        public ICollection<SystemFile> Files { get; set; }

        [HasOne]
        public SystemDirectory Self { get; set; }

        [HasOne]
        public SystemDirectory AlsoSelf { get; set; }

        [HasOne] 
        public SystemDirectory Parent { get; set; }
    }
}
