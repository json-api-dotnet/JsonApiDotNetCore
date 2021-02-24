using System;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    public sealed class ResourceWithThrowingConstructor : Identifiable
    {
        public ResourceWithThrowingConstructor()
        {
            throw new ArgumentException("Failed to initialize.");
        }
    }
}
