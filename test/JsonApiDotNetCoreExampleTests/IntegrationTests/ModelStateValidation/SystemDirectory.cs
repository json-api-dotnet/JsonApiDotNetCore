using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SystemDirectory : Identifiable
    {
        [Required]
        [RegularExpression("^[0-9]+$")]
        public override int Id { get; set; }

        [Attr]
        [Required]
        [RegularExpression(@"^[\w\s]+$")]
        public string Name { get; set; }

        [Attr]
        [Required]
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
