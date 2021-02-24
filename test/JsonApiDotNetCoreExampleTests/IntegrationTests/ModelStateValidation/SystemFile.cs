using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SystemFile : Identifiable
    {
        [Attr]
        [Required]
        [MinLength(1)]
        public string FileName { get; set; }

        [Attr]
        [Required]
        [Range(typeof(long), "0", "9223372036854775807")]
        public long SizeInBytes { get; set; }
    }
}
