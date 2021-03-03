using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.ResourceHooks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Dummy : Identifiable
    {
        public string SomeUpdatedProperty { get; set; }
        public string SomeNotUpdatedProperty { get; set; }

        [HasOne]
        public ToOne FirstToOne { get; set; }

        [HasOne]
        public ToOne SecondToOne { get; set; }

        [HasMany]
        public ISet<ToMany> ToManies { get; set; }
    }
}
