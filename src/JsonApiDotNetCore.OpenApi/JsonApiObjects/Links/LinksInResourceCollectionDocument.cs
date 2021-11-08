using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class LinksInResourceCollectionDocument
    {
        [Required]
        public string Self { get; set; }

        public string Describedby { get; set; }

        [Required]
        public string First { get; set; }

        public string Last { get; set; }

        public string Prev { get; set; }

        public string Next { get; set; }
    }
}
