using System;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public class Cat : Animal
    {
        public Cat()
        {
            Feline = true;
        }

        [Attr]
        public bool Meows { get; set; }
    }
}
