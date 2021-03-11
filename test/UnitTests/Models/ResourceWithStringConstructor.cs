using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class ResourceWithStringConstructor : Identifiable
    {
        public string Text { get; }

        public ResourceWithStringConstructor(string text)
        {
            ArgumentGuard.NotNullNorEmpty(text, nameof(text));

            Text = text;
        }
    }
}
