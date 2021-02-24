using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Book : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public int PublishYear { get; set; }

        [HasOne]
        public Person Author { get; set; }
    }
}
