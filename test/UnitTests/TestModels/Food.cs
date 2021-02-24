using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class Food : Identifiable
    {
        [Attr] public string Dish { get; set; }
    }
}
