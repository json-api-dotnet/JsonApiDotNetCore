using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class Food : Identifiable
    {
        [Attr]
        public string Dish { get; set; }
    }
}
