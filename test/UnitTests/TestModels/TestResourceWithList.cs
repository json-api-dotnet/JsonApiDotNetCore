using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class TestResourceWithList : Identifiable
    {
        [Attr] public List<ComplexType> ComplexFields { get; set; }
    }
}