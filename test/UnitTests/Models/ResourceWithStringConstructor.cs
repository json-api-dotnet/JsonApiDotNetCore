using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;

namespace UnitTests.Models
{
    public class ResourceWithStringConstructor : Identifiable
    {
        public string Text { get; }

        public ResourceWithStringConstructor(string text)
        {
            ArgumentGuard.NotNull(text, nameof(text));

            Text = text;
        }
    }
}