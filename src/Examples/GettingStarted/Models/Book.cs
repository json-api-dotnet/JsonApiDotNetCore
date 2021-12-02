using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource]
    public sealed class Book : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [Attr]
        public int PublishYear { get; set; }

        [HasOne]
        public Person Author { get; set; } = null!;
    }
}
