using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceWithThrowingConstructor : Identifiable
    {
        public ResourceWithThrowingConstructor()
        {
            throw new ArgumentException("Failed to initialize.");
        }
    }
}
