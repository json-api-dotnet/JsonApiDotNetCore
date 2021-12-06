using System.Text.Json;
using FluentAssertions.Execution;

namespace TestBuildingBlocks
{
    public sealed class JsonElementAssertions : JsonElementAssertions<JsonElementAssertions>
    {
        internal JsonElementAssertions(JsonElement subject)
            : base(subject)
        {
        }
    }

    public class JsonElementAssertions<TAssertions>
        where TAssertions : JsonElementAssertions<TAssertions>
    {
        /// <summary>
        /// - Gets the object which value is being asserted.
        /// </summary>
        private JsonElement Subject { get; }

        protected JsonElementAssertions(JsonElement subject)
        {
            Subject = subject;
        }

        public void HaveProperty(string propertyName, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion.ForCondition(Subject.TryGetProperty(propertyName, out _)).BecauseOf(because, becauseArgs)
                .FailWith($"Expected element to have property with name '{propertyName}, but did not find it.");
        }
    }
}
