using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ResourceWithStringConstructor : Identifiable
    {
        public string Text { get; }

        public ResourceWithStringConstructor(string text)
        {
            ArgumentGuard.NotNull(text, nameof(text));

            Text = text;
        }
    }
}
