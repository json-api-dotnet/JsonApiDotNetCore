using System;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    public class ResourceWithThrowingConstructor : Identifiable
    {
        public ResourceWithThrowingConstructor()
        {
            throw new ArgumentException("Failed to initialize.");
        }
    }
}
