using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestResource : Identifiable
    {
        [Attr]
        public string StringField { get; set; }

        [Attr]
        public DateTime DateTimeField { get; set; }

        [Attr]
        public DateTime? NullableDateTimeField { get; set; }

        [Attr]
        public int IntField { get; set; }

        [Attr]
        public int? NullableIntField { get; set; }

        [Attr]
        public Guid GuidField { get; set; }

        [Attr]
        public ComplexType ComplexField { get; set; }
    }
}
