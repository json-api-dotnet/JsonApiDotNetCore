using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Person : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
