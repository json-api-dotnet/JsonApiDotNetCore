using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DivingBoard : Identifiable
    {
        [Attr]
        [Required]
        [Range(1, 20)]
        public decimal HeightInMeters { get; set; }
    }
}
