using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class SystemFile : Identifiable
    {
        [Attr]
        [IsRequired]
        [MinLength(1)]
        public string FileName { get; set; }

        [Attr]
        [IsRequired]
        [Range(typeof(long), "0", "9223372036854775807")]
        public long SizeInBytes { get; set; }
    }
}
