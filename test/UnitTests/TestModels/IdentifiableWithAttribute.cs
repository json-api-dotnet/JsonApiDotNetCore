using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class IdentifiableWithAttribute : Identifiable
    {
        [Attr]
        public string AttributeMember { get; set; }
    }
}
