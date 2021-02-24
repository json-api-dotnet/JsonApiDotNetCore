using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class Song : Identifiable
    {
        [Attr] public string Title { get; set; }
    }
}
