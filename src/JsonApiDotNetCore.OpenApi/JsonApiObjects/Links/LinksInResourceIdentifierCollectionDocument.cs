using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class LinksInResourceIdentifierCollectionDocument
    {
        [Required]
        public string Self { get; set; }

        public string Describedby { get; set; }

        [Required]
        public string Related { get; set; }

        [Required]
        public string First { get; set; }

        public string Last { get; set; }

        public string Prev { get; set; }

        public string Next { get; set; }
    }
}
