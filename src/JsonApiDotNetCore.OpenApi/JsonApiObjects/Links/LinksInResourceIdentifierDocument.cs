using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class LinksInResourceIdentifierDocument
    {
        [Required]
        public string Self { get; set; }

        public string Describedby { get; set; }

        [Required]
        public string Related { get; set; }
    }
}
