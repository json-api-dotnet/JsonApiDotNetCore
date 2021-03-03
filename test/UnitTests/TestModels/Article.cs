using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Article : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public Person Reviewer { get; set; }

        [HasOne]
        public Person Author { get; set; }

        [HasOne(CanInclude = false)]
        public Person CannotInclude { get; set; }
    }
}
